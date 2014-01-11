using System;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace fomm.NifViewer {
    public partial class NifFile {
        private Subset[] subsets;
        private float radius;
        private readonly Device device;

        private bool disposed=false;

        private int[] AlphaIndicies;

        public float Radius { get { return radius; } }
        public int Subsets { get { return subsets.Length; } }

        public static string loadLog="";

        private void AlphaSort() {
            bool found=true;
            float[] len=new float[AlphaIndicies.Length];
            for(int i=0;i<len.Length;i++) len[i]=Vector3.Length(Vector3.TransformCoordinate(subsets[AlphaIndicies[i]].info.Center,camera.WorldMatrix)-camera.EyeLocation);
            while(found) {
                found=false;
                for(int i=0;i<len.Length-1;i++) {
                    if(len[i]>len[i+1]) {
                        float f=len[i];
                        len[i]=len[i+1];
                        len[i+1]=f;
                        int j=AlphaIndicies[i];
                        AlphaIndicies[i]=AlphaIndicies[i+1];
                        AlphaIndicies[i+1]=j;
                        found=true;
                    }
                }
            }
        }

        private unsafe void BasicSetup(byte[] data, string tex0) {
            LoadReturn lr=Load(data, data.Length);
            if(lr.Subsets<=0) throw new ApplicationException("Failed to load nif");
            subsets=new Subset[lr.Subsets];
            radius=lr.maxsize;
            loadLog=new string(lr.log);
            if(lr.FailedSubsets>0) System.Windows.Forms.MessageBox.Show(""+lr.FailedSubsets+" could not be rendered", "Warning");

            string texFileName;
            for(uint i=0;i<subsets.Length;i++) {
                //Get basic info
                GetInfo(i, out subsets[i].info);
                GetMaterial(i, out subsets[i].mat);
                GetTransform(i, out subsets[i].transform);
                if(i==0&&tex0!=null) texFileName=tex0;
                else if(subsets[i].info.containsTexture) {
                    sbyte* charPtr;
                    GetTex(i, out charPtr);
                    texFileName=new string(charPtr);
                } else texFileName=null;

                //sanity check
                //if(!subsets[i].info.containsTexCoords) throw new ApplicationException("Vertex texture coords are missing from subset "+i);
                //if(!subsets[i].info.containsNormals) throw new ApplicationException("Vertex normals are missing from subset "+i);
                //if(texFileName==null) throw new ApplicationException("No texture name was specified in subset "+i);

                //Load textures
                if(texFileName!=null) {
                    System.IO.Stream stream=BSAArchive.GetTexture(texFileName);
                    if(stream!=null) {
                        subsets[i].colorMap=TextureLoader.FromStream(device, stream);
                        SurfaceDescription desc=subsets[i].colorMap.GetLevelDescription(0);
                        subsets[i].data.cWidth=desc.Width;
                        subsets[i].data.cHeight=desc.Height;
                        subsets[i].data.cformat=desc.Format;
                        subsets[i].data.path=texFileName;
                    } else throw new ApplicationException("Could not load texture for subset "+i);
                    stream=BSAArchive.GetGlowTexture(texFileName);
                    if(stream!=null) {
                        subsets[i].glowMap=TextureLoader.FromStream(device, stream);
                        SurfaceDescription desc=subsets[i].glowMap.GetLevelDescription(0);
                        subsets[i].data.gWidth=desc.Width;
                        subsets[i].data.gHeight=desc.Height;
                        subsets[i].data.gformat=desc.Format;
                    } else subsets[i].data.gWidth=-1;
                    stream=BSAArchive.GetNormalTexture(texFileName);
                    if(stream!=null) {
                        subsets[i].normalMap=TextureLoader.FromStream(device, stream);
                        SurfaceDescription desc=subsets[i].normalMap.GetLevelDescription(0);
                        subsets[i].data.nWidth=desc.Width;
                        subsets[i].data.nHeight=desc.Height;
                        subsets[i].data.nformat=desc.Format;
                    } else subsets[i].data.nWidth=-1;
                } else {
                    subsets[i].colorMap=null;
                    subsets[i].data.cWidth=-1;
                    subsets[i].data.gWidth=-1;
                    subsets[i].data.nWidth=-1;
                }

                //Get vertex information
                VertexElement[] decl=new VertexElement[7];
                decl[0]=new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0);
                decl[1]=new VertexElement(0, 12, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Tangent, 0);
                decl[2]=new VertexElement(0, 24, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.BiNormal, 0);
                decl[3]=new VertexElement(0, 36, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0);
                decl[4]=new VertexElement(0, 48, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0);
                decl[5]=new VertexElement(0, 56, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.Color, 0);
                decl[6]=VertexElement.VertexDeclarationEnd;
                subsets[i].vDecl=new VertexDeclaration(device, decl);

                if(subsets[i].info.containsTangentBinormal) {
                    Mesh m=new Mesh(subsets[i].info.iCount/3, subsets[i].info.vCount, MeshFlags.SystemMemory, decl, device);

                    subsets[i].vBuffer=new VertexBuffer(typeof(ConvertedVertex), subsets[i].info.vCount, device, Usage.WriteOnly, VertexFormats.None, Pool.Managed);
                    //ConvertedVertex[] cv=(ConvertedVertex[])subsets[i].vBuffer.Lock(0, LockFlags.None);
                    ConvertedVertex[] cv=(ConvertedVertex[])m.LockVertexBuffer(typeof(ConvertedVertex), LockFlags.None, subsets[i].info.vCount);
                    fixed(ConvertedVertex* cvPtr = cv) {
                        GetVerticies(i, cvPtr);
                    }
                    m.UnlockVertexBuffer();
                    //subsets[i].vBuffer.Unlock();


                    //Indicies
                    subsets[i].iBuffer=new IndexBuffer(typeof(ushort), subsets[i].info.iCount, device, Usage.WriteOnly, Pool.Managed);
                    //ushort[] indicies=(ushort[])subsets[i].iBuffer.Lock(0, LockFlags.None);
                    ushort[] indicies=(ushort[])m.LockIndexBuffer(typeof(ushort), LockFlags.None, subsets[i].info.iCount);
                    fixed(ushort* iPtr = indicies) {
                        GetIndicies(i, iPtr);
                    }
                    //subsets[i].iBuffer.Unlock();
                    m.UnlockIndexBuffer();
                    subsets[i].numTris=subsets[i].info.iCount/3;

                    int[] adj=new int[subsets[i].info.iCount];
                    m.GenerateAdjacency(0.01f, adj);
                    m.ComputeTangent(0, 0, 0, 1, adj);

                    //m.ComputeTangentFrame(TangentOptions.CalculateNormals|TangentOptions.GenerateInPlace);

                    cv=(ConvertedVertex[])m.LockVertexBuffer(typeof(ConvertedVertex), LockFlags.ReadOnly, subsets[i].info.vCount);
                    ConvertedVertex[] cv2=(ConvertedVertex[])subsets[i].vBuffer.Lock(0, LockFlags.None);
                    for(int j=0;j<cv.Length;j++) {
                        cv2[j]=cv[j];
                    }
                    m.UnlockVertexBuffer();
                    subsets[i].vBuffer.Unlock();

                    indicies=(ushort[])m.LockIndexBuffer(typeof(ushort), LockFlags.None, subsets[i].info.iCount);
                    ushort[] indicies2=(ushort[])subsets[i].iBuffer.Lock(0, LockFlags.None);
                    for(int j=0;j<indicies.Length;j++) {
                        indicies2[j]=indicies[j];
                    }
                    m.UnlockIndexBuffer();
                    subsets[i].iBuffer.Unlock();

                    m.Dispose();

                } else {
                    subsets[i].vBuffer=new VertexBuffer(typeof(ConvertedVertex), subsets[i].info.vCount, device, Usage.WriteOnly, VertexFormats.None, Pool.Managed);
                    ConvertedVertex[] cv=(ConvertedVertex[])subsets[i].vBuffer.Lock(0, LockFlags.None);
                    fixed(ConvertedVertex* cvPtr = cv) {
                        GetVerticies(i, cvPtr);
                    }
                    subsets[i].vBuffer.Unlock();

                    //Indicies
                    subsets[i].iBuffer=new IndexBuffer(typeof(ushort), subsets[i].info.iCount, device, Usage.WriteOnly, Pool.Managed);
                    ushort[] indicies=(ushort[])subsets[i].iBuffer.Lock(0, LockFlags.None);
                    fixed(ushort* iPtr = indicies) {
                        GetIndicies(i, iPtr);
                    }
                    subsets[i].iBuffer.Unlock();
                    subsets[i].numTris=subsets[i].info.iCount/3;
                }
            }

            int alphaCount=0;
            for(int i=0;i<Subsets;i++) if(subsets[i].info.hasalpha) alphaCount++;
            AlphaIndicies=new int[alphaCount];
            alphaCount=0;
            for(int i=0;i<Subsets;i++) if(subsets[i].info.hasalpha) AlphaIndicies[alphaCount++]=i;
        }

        public NifFile(string path, Device _device) {
            device=_device;
            //Load nif file
            byte[] data=System.IO.File.ReadAllBytes(path);
            BasicSetup(data, null);
        }

        public NifFile(byte[] data, Device _device) {
            device=_device;
            BasicSetup(data, null);
        }

        public NifFile(string path, Device _device, string tex0) {
            device=_device;
            //Load nif file
            byte[] data=System.IO.File.ReadAllBytes(path);
            BasicSetup(data, tex0);
        }

        public NifFile(byte[] data, Device _device, string tex0) {
            device=_device;
            //Load nif file
            BasicSetup(data, tex0);
        }

        public void Dispose() {
            if(disposed) return;

            for(int i=0;i<subsets.Length;i++) {
                subsets[i].vBuffer.Dispose();
                subsets[i].vDecl.Dispose();
                if(subsets[i].iBuffer!=null) {
                        if(subsets[i].iBuffer!=null) subsets[i].iBuffer.Dispose();
                }
                if(subsets[i].colorMap!=null) subsets[i].colorMap.Dispose();
                if(subsets[i].oldColorMap!=null) subsets[i].oldColorMap.Dispose();
                if(subsets[i].normalMap!=null) subsets[i].normalMap.Dispose();
                if(subsets[i].glowMap!=null) subsets[i].glowMap.Dispose();
            }

            disposed=true;
        }

        public void RenderSubset(int i) {
            Matrix world=subsets[i].transform * camera.WorldMatrix;
            effect.SetValue(ehWorld, world);
            //effect.SetValue(ehIWorld, Matrix.Invert(camera.WorldMatrix));
            //world*=BasicHLSL.ViewMatrix;
            //effect.SetValue(ehWorldView, world);
            effect.SetValue(ehWorldViewProj, world * camera.ViewMatrix * camera.ProjectionMatrix);
            //Stricktly speaking, normal matrix should be transpose(invert(world * view)).
            //Could probably just use the world matrix instead
            world.Invert();
            world=Matrix.TransposeMatrix(world);
            effect.SetValue(ehNormalMatrix, world);

            effect.SetValue(ehTexture, subsets[i].colorMap);
            effect.SetValue(ehMaterialAmb, subsets[i].mat.AmbientColor);
            effect.SetValue(ehMaterialDif, subsets[i].mat.DiffuseColor);
            effect.SetValue(ehMaterialSpec, subsets[i].mat.SpecularColor);
            effect.SetValue(ehMaterialEmm, subsets[i].mat.EmissiveColor);
            effect.SetValue(ehNormalMap, subsets[i].normalMap);
            effect.SetValue(ehGlowMap, subsets[i].glowMap);
            effect.SetValue(ehMaterialGloss, subsets[i].mat.SpecularSharpness);

            effect.SetValue(ehUseDiffuseVColor, false);
            effect.SetValue(ehUseEmissiveVColor, false);
            effect.SetValue(ehUseGlow, false);
            effect.SetValue(ehUseSpecular, false);

            if(subsets[i].info.wireframe||BasicHLSL.Wireframe) device.RenderState.FillMode=FillMode.WireFrame;
            else device.RenderState.FillMode=FillMode.Solid;

            effect.SetValue(ehMaterialAlpha, subsets[i].info.alpha);
            effect.SetValue(ehUseExplicitAlpha, subsets[i].info.hasalpha);

            if(SetTechnique) {
                string tech;
                if(subsets[i].colorMap!=null) {
                    if(subsets[i].normalMap!=null) {
                        if(subsets[i].data.nformat!=Format.Dxt1&&subsets[i].data.nformat!=Format.R8G8B8) {
                            effect.SetValue(ehUseSpecular, true);
                        }
                        if(ParallaxEnabled) {
                            switch(subsets[i].info.applyMode) {
                            case 4:
                                tech="Parallax";
                                break;
                            case 5:
                                tech="Relief";
                                break;
                            case 6:
                                tech="Fur";
                                break;
                            default:
                                tech="Normal";
                                break;
                            }
                        } else tech="Normal";
                    } else tech="Flat";
                    if(subsets[i].glowMap!=null) effect.SetValue(ehUseGlow, true);
                } else tech="Textureless";
                if(subsets[i].info.containsColor) effect.SetValue(ehUseDiffuseVColor, true);
                effect.Technique=tech;
            }

            device.SetStreamSource(0, subsets[i].vBuffer, 0);
            device.VertexDeclaration=subsets[i].vDecl;

            int passes=effect.Begin(FX.None);
            for(int pass=0;pass<passes;pass++) {
                effect.BeginPass(pass);
                if(!subsets[i].info.containsIndicies) {
                    device.Indices=null;
                    device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, subsets[i].info.vCount-2);
                } else {
                        device.Indices=subsets[i].iBuffer;
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, subsets[i].info.vCount, 0, subsets[i].numTris);
                }
                effect.EndPass();
            }
            effect.End();
        }

        public void Render() {
            if(disposed) throw new ObjectDisposedException("NifFile");
            for(int i=0;i<subsets.Length;i++) if(!subsets[i].info.hasalpha) RenderSubset(i);
            //for(int i=0;i<subsets.Length;i++) if(subsets[i].info.hasalpha) RenderSubset(i);
            if(AlphaIndicies.Length>0) {
                AlphaSort();
                for(int i=0;i<AlphaIndicies.Length;i++) RenderSubset(AlphaIndicies[i]);
            }
        }

        public SubsetData GetSubsetData(int subset) {
            return subsets[subset].data;
        }

        public byte GetApplyMode(int i) {
            return subsets[i].info.applyMode;
        }

        public void SetApplyMode(int i, byte mode) {
            subsets[i].info.applyMode=mode;
        }

        private static Microsoft.Samples.DirectX.UtilityToolkit.ModelViewerCamera camera;

        private static Effect effect;
        private static EffectHandle ehWorld;
        private static EffectHandle ehIWorld;
        private static EffectHandle ehWorldViewProj;
        private static EffectHandle ehWorldView;
        private static EffectHandle ehNormalMatrix;
        private static EffectHandle ehTexture;
        private static EffectHandle ehGlowMap;
        private static EffectHandle ehNormalMap;
        private static EffectHandle ehMaterialAmb;
        private static EffectHandle ehMaterialDif;
        private static EffectHandle ehMaterialSpec;
        private static EffectHandle ehMaterialEmm;
        private static EffectHandle ehMaterialGloss;
        private static EffectHandle ehMaterialAlpha;

        private static EffectHandle ehUseSpecular;
        private static EffectHandle ehUseGlow;
        private static EffectHandle ehUseDiffuseVColor;
        private static EffectHandle ehUseEmissiveVColor;
        private static EffectHandle ehUseExplicitAlpha;

        private static bool ParallaxEnabled=true;
        private static bool SetTechnique=true;

        public static void SetEffect(Effect _effect) {
            effect=_effect;
            if(effect!=null) {
                ehWorld=effect.GetParameter(null, "worldMatrix");
                ehIWorld=effect.GetParameter(null, "iWorld");
                ehNormalMatrix=effect.GetParameter(null, "normalMatrix");
                ehWorldView=effect.GetParameter(null, "worldView");
                ehWorldViewProj=effect.GetParameter(null, "worldViewProj");
                ehTexture=effect.GetParameter(null, "colorMap");
                ehGlowMap=effect.GetParameter(null, "glowMap");
                ehNormalMap=effect.GetParameter(null, "normalMap");
                ehMaterialAmb=effect.GetParameter(null, "g_MaterialAmbientColor");
                ehMaterialDif=effect.GetParameter(null, "g_MaterialDiffuseColor");
                ehMaterialSpec=effect.GetParameter(null, "g_MaterialSpecularColor");
                ehMaterialEmm=effect.GetParameter(null, "g_MaterialEmissiveColor");
                ehMaterialGloss=effect.GetParameter(null, "g_MaterialGloss");
                ehMaterialAlpha=effect.GetParameter(null, "g_MaterialAlpha");
                ehUseSpecular=effect.GetParameter(null, "use_specular");
                ehUseGlow=effect.GetParameter(null, "use_glow");
                ehUseDiffuseVColor=effect.GetParameter(null, "use_dvcolor");
                ehUseEmissiveVColor=effect.GetParameter(null, "use_evcolor");
                ehUseExplicitAlpha=effect.GetParameter(null, "use_alpha");
            }
        }

        public static void SetCamera(Microsoft.Samples.DirectX.UtilityToolkit.ModelViewerCamera c) {
            camera=c;
        }

        public static void SetParallax(bool On) {
            ParallaxEnabled=On;
        }

        public static void SetAutoTechnique(bool On) {
            SetTechnique=On;
        }

        public void SaveNif(string outpath) {
            byte[] parallaxed=new byte[subsets.Length];
            for(int i=0;i<subsets.Length;i++) {
                parallaxed[i]=subsets[i].info.applyMode;
            }
            System.Windows.Forms.SaveFileDialog sfd=new System.Windows.Forms.SaveFileDialog();
            sfd.RestoreDirectory=true;
            sfd.InitialDirectory=System.IO.Path.GetDirectoryName(outpath);
            sfd.FileName=System.IO.Path.GetFileName(outpath);
            sfd.Filter="Nif file (*.nif)|*.nif";
            if(sfd.ShowDialog()!=System.Windows.Forms.DialogResult.OK) return;
            ResaveNif(sfd.FileName, parallaxed);
        }

        public void SaveColorMap(int subset) {
            if(subsets[subset].colorMap==null) System.Windows.Forms.MessageBox.Show("No texture available to save", "Error");
            string texpath=subsets[subset].data.path;
            if(!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(texpath))) System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(texpath));
            System.Windows.Forms.SaveFileDialog sfd=new System.Windows.Forms.SaveFileDialog();
            sfd.RestoreDirectory=true;
            sfd.InitialDirectory=System.IO.Path.GetDirectoryName(texpath);
            sfd.FileName=System.IO.Path.GetFileNameWithoutExtension(texpath)+".dds";
            sfd.Filter="DirectX surface (*.dds)|*.dds";
            if(sfd.ShowDialog()!=System.Windows.Forms.DialogResult.OK) return;
            TextureLoader.Save(sfd.FileName, ImageFileFormat.Dds, subsets[subset].colorMap);
        }
    }
}