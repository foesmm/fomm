using System;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.Samples.DirectX.UtilityToolkit;
using MessageBox=System.Windows.Forms.MessageBox;
using Path=System.IO.Path;
using Directory=System.IO.Directory;
using File=System.IO.File;

namespace fomm.NifViewer {
    public class BasicHLSL : IFrameworkCallback, IDeviceCreation {
        private System.Windows.Forms.ColorDialog colorDialog;

        public BasicHLSL(Framework f) {
            sampleFramework = f;
            hud = new Dialog(sampleFramework);
            sampleUi = new Dialog(sampleFramework);

            colorDialog=new System.Windows.Forms.ColorDialog();
            colorDialog.FullOpen=true;
            colorDialog.ShowHelp=false;
            colorDialog.SolidColorOnly=false;
            colorDialog.AnyColor=true;
        }

        // static stuff
        private static ColorValue LightColor = new ColorValue(1.0f, 1.0f, 1.0f, 1.0f);
        private static ColorValue AmbientLightColor = new ColorValue(1.0f, 1.0f, 1.0f, 1.0f);

        private static int AA;
        private static int AF;

        public static Device Device { get { return sampleFramework.Device; } }
        public static void Hide() { sampleFramework.Window.Visible=false; }
        public static void Show() { sampleFramework.Window.Visible=true; }

        private static bool wireframe=false;
        public static bool Wireframe { get { return wireframe; } }

        // Variables
        private static Framework sampleFramework = null; // Framework for samples
        private Font statsFont = null;      // Font for drawing text
        private Sprite textSprite = null;   // Sprite for batching text calls
        private Effect effect = null;       // D3DX Effect Interface
        private ModelViewerCamera camera = new ModelViewerCamera(); // A model viewing camera
        private bool isHelpShowing = false; // If true, renders the UI help text
        private bool isLogShowing = false;
        private Dialog hud;                 // dialog for standard controls
        private Dialog sampleUi;            // dialog for sample specific controls
        private NifFile nif;                //The nif file we're displaying
        private int currentSubset=-1;
        private string subsetDesc;
        private string nifPath;

        private EffectHandle ehViewProj;
        private EffectHandle ehLightDir;
        private EffectHandle ehLightCol;
        private EffectHandle ehHalfVec;
        private EffectHandle egAmbCol;
        private EffectHandle ehEyePos;
        private EffectHandle ehEyeVec;

        // Light stuff
        DirectionWidget lightControl;
        private float lightScale;
        private float ambLightScale;

        // HUD Ui Control constants
        private const int ToggleFullscreen = 1;
        private const int ChangeDevice = 3;
        
        private const int LightScaleControl = 10;
        private const int LightScaleStatic = 11;
        private const int LightColorPicker = 12;
        private const int AmbLightScaleControl = 13;
        private const int AmbLightScaleStatic = 14;
        private const int AmbLightColorPicker = 15;
        private const int ChangeShader = 16;
        private const int ToggleCulling = 2;
        private const int WireFrame = 18;
        private const int SubsetControl = 19;
        private const int DirectCheckbox = 20;
        private const int LoadMeshButton = 24;


        /// <summary>
        /// Called during device initialization, this code checks the device for some 
        /// minimum set of capabilities, and rejects those that don't pass by returning false.
        /// </summary>
        public bool IsDeviceAcceptable(Caps caps, Format adapterFormat, Format backBufferFormat, bool windowed) {
            // No fallback defined by this app, so reject any device that 
            // doesn't support at least ps1.1
            if(caps.PixelShaderVersion < new Version(1, 1))
                return false;

            // Skip back buffer formats that don't support alpha blending
            if(!Manager.CheckDeviceFormat(caps.AdapterOrdinal, caps.DeviceType, adapterFormat,
                Usage.QueryPostPixelShaderBlending, ResourceType.Textures, backBufferFormat))
                return false;

            return true;
        }

        /// <summary>
        /// This callback function is called immediately before a device is created to allow the 
        /// application to modify the device settings. The supplied settings parameter 
        /// contains the settings that the framework has selected for the new device, and the 
        /// application can make any desired changes directly to this structure.  Note however that 
        /// the sample framework will not correct invalid device settings so care must be taken 
        /// to return valid device settings, otherwise creating the Device will fail.  
        /// </summary>
        public void ModifyDeviceSettings(DeviceSettings settings, Caps caps) {
            // If device doesn't support 2.0 pixel shaders, switch to reference device
            if(caps.PixelShaderVersion < new Version(2, 0)) {
                settings.DeviceType=DeviceType.Reference;
                settings.BehaviorFlags = CreateFlags.SoftwareVertexProcessing;

                MessageBox.Show("Your graphics card does not support the required pixel shader version.\n"+
                    "Switching to reference mod (expect a maximum fps of ~1)","Warning");

                if(AA!=0 && AA!= 4 && AA!=9) {
                    MessageBox.Show("Your chosen antialiasing setting is unsupported by the reference rasterizer.\n"+
                        "Defaulting to off", "Warning");
                    AA=0;
                }

                if(AF!=0 && AF!=2 && AF!=4 && AF!=8 && AF!=16) {
                    MessageBox.Show("Your chosen anisotropic filtering setting is unsupported by the reference rasterizer.\n"+
                        "Defaulting to linear filters","Warning");
                    AF=0;
                }
            } else {
                // If device doesn't support HW T&L or doesn't support 2.0 vertex shaders in HW then switch to SWVP.
                if((!caps.DeviceCaps.SupportsHardwareTransformAndLight) || (caps.VertexShaderVersion < new Version(2, 0))) {
                    settings.BehaviorFlags = CreateFlags.SoftwareVertexProcessing;
                } else {
                    settings.BehaviorFlags = CreateFlags.HardwareVertexProcessing;
                }

                // This application is designed to work on a pure device by not using 
                // any get methods, so create a pure device if supported and using HWVP.
                if((caps.DeviceCaps.SupportsPureDevice) && ((settings.BehaviorFlags & CreateFlags.HardwareVertexProcessing) != 0))
                    settings.BehaviorFlags |= CreateFlags.PureDevice;

                settings.BehaviorFlags |= CreateFlags.MultiThreaded; //Because .NET has a habit of randomly swapping threads
                if(AF!=0) {
                    if((AF!=2 && AF!=4 && AF!=8 && AF!=16) || AF>caps.MaxAnisotropy) {
                        MessageBox.Show("Your chosen anisotropic filtering setting is unsupported by your graphics hardware.\n"+
                            "Defaulting to linear filters","Warning");
                        AF=0;
                    }
                }
            }

            settings.presentParams.MultiSample=(MultiSampleType)AA;
        }

        /// <summary>
        /// This event will be fired immediately after the Direct3D device has been 
        /// created, which will happen during application initialization and windowed/full screen 
        /// toggles. This is the best location to create Pool.Managed resources since these 
        /// resources need to be reloaded whenever the device is destroyed. Resources created  
        /// here should be released in the Disposing event. 
        /// </summary>
        private void OnCreateDevice(object sender, DeviceEventArgs e) {
            // Setup direction widget
            DirectionWidget.OnCreateDevice(e.Device);

            // Initialize the stats font
            statsFont = ResourceCache.GetGlobalInstance().CreateFont(e.Device, 15, 0, FontWeight.Bold, 1, false, CharacterSet.Default,
                Precision.Default, FontQuality.Default, PitchAndFamily.FamilyDoNotCare | PitchAndFamily.DefaultPitch
                , "Arial");

            // Read the D3DX effect file
            string path = "NifViewer.fx";
            string errors;
            effect = ResourceCache.GetGlobalInstance().CreateEffectFromFile(e.Device, path, null, null, ShaderFlags.NotCloneable, null, out errors);

            if(effect==null) {
                MessageBox.Show("Effects.fx Shader compilation failed.\n"+errors, "Error");
            }

            ehLightDir=effect.GetParameter(null, "g_LightDir");
            ehLightCol=effect.GetParameter(null, "g_LightDiffuse");
            egAmbCol=effect.GetParameter(null, "g_LightAmbient");
            ehViewProj=effect.GetParameter(null, "viewProjection");
            ehEyePos=effect.GetParameter(null, "eyePos");
            ehEyeVec=effect.GetParameter(null, "eyeVec");
            ehHalfVec=effect.GetParameter(null, "g_LightHalfVec");

            NifFile.SetEffect(effect);

            // Setup the camera's view parameters
            camera.SetViewParameters(new Vector3(0.0f, 0.0f, -15.0f), Vector3.Empty);
            camera.IsPositionMovementEnabled=true;

            NifFile.SetCamera(camera);

            lightControl.Radius = 10;
            camera.SetRadius(30.0f, 0, 100.0f);
            
        }

        /// <summary>
        /// This event will be fired immediately after the Direct3D device has been 
        /// reset, which will happen after a lost device scenario. This is the best location to 
        /// create Pool.Default resources since these resources need to be reloaded whenever 
        /// the device is lost. Resources created here should be released in the OnLostDevice 
        /// event. 
        /// </summary>
        private void OnResetDevice(object sender, DeviceEventArgs e) {
            SurfaceDescription desc = e.BackBufferDescription;
            // Create a sprite to help batch calls when drawing many lines of text
            textSprite = new Sprite(e.Device);

            // Reset items
            lightControl.OnResetDevice(desc);

            // Setup the camera's projection parameters
            float aspectRatio = (float)desc.Width / (float)desc.Height;
            if(nif!=null) {
                camera.SetProjectionParameters((float)Math.PI / 4, aspectRatio, nif.Radius/50, nif.Radius*5);
            } else {
                camera.SetProjectionParameters((float)Math.PI / 4, aspectRatio, 10, 4000);
            }
            camera.SetWindow(desc.Width, desc.Height);
            camera.SetButtonMasks((int)MouseButtonMask.Left, (int)MouseButtonMask.Wheel, (int)MouseButtonMask.Middle);

            // Setup UI locations
            hud.SetLocation(desc.Width-170, 0);
            hud.SetSize(170, 170);
            sampleUi.SetLocation(desc.Width - 170, 60);
            sampleUi.SetSize(170, 300);

            e.Device.RenderState.ZBufferFunction=Compare.Less;
            if(AF==0) {
                for(int i=0;i<3;i++) {
                    e.Device.SamplerState[i].MagFilter=TextureFilter.Linear;
                    e.Device.SamplerState[i].MinFilter=TextureFilter.Linear;
                }
            } else {
                for(int i=0;i<3;i++) {
                    e.Device.SamplerState[i].MaxAnisotropy=AF;
                    e.Device.SamplerState[i].MinFilter=TextureFilter.Anisotropic;
                    e.Device.SamplerState[i].MagFilter=TextureFilter.Linear;
                }
            }

            e.Device.RenderState.AlphaBlendEnable=true;
            e.Device.RenderState.AlphaTestEnable=true;
            e.Device.RenderState.AlphaFunction=Compare.Greater;
            e.Device.RenderState.SourceBlend=Blend.SourceAlpha;
            e.Device.RenderState.DestinationBlend=Blend.InvSourceAlpha;
        }

        /// <summary>
        /// This event function will be called fired after the Direct3D device has 
        /// entered a lost state and before Device.Reset() is called. Resources created
        /// in the OnResetDevice callback should be released here, which generally includes all 
        /// Pool.Default resources. See the "Lost Devices" section of the documentation for 
        /// information about lost devices.
        /// </summary>
        private void OnLostDevice(object sender, EventArgs e) {
            if(textSprite != null) {
                textSprite.Dispose();
                textSprite = null;
            }

            // Update the direction widget
            DirectionWidget.OnLostDevice();
        }

        /// <summary>
        /// This event will be fired immediately after the Direct3D device has 
        /// been destroyed, which generally happens as a result of application termination or 
        /// windowed/full screen toggles. Resources created in the OnCreateDevice event 
        /// should be released here, which generally includes all Pool.Managed resources. 
        /// </summary>
        private void OnDestroyDevice(object sender, EventArgs e) {
            // Update the direction widget
            DirectionWidget.OnDestroyDevice();
            if(nif != null) {
                nif.Dispose();
                nif=null;
                nifPath=null;
                currentSubset=-1;
            }
        }

        /// <summary>
        /// This callback function will be called once at the beginning of every frame. This is the
        /// best location for your application to handle updates to the scene, but is not 
        /// intended to contain actual rendering calls, which should instead be placed in the 
        /// OnFrameRender callback.  
        /// </summary>
        public void OnFrameMove(Device device, double appTime, float elapsedTime) {
            // Update the camera's position based on user input 
            camera.FrameMove(elapsedTime);
        }

        /// <summary>
        /// This callback function will be called at the end of every frame to perform all the 
        /// rendering calls for the scene, and it will also be called if the window needs to be 
        /// repainted. After this function has returned, the sample framework will call 
        /// Device.Present to display the contents of the next buffer in the swap chain
        /// </summary>
        public void OnFrameRender(Device device, double appTime, float elapsedTime) {
            bool beginSceneCalled = false;

            // Clear the render target and the zbuffer 
            device.Clear(ClearFlags.ZBuffer | ClearFlags.Target, unchecked((int)0x8C003F3F), 1.0f, 0);
            try {
                device.BeginScene();
                beginSceneCalled = true;

                // Render the arrows so the user can visually see the light direction
                ColorValue arrowColor = LightColor;
                lightControl.OnRender(arrowColor, camera.ViewMatrix, camera.ProjectionMatrix, camera.EyeLocation);

                if(nif!=null) {
                    // Update the effects now
                    Vector3 ldir=lightControl.LightDirection;
                    Vector3 edir=-camera.EyeVector;
                    Vector3 hdir=ldir+edir;
                    hdir.Normalize();
                    float[] dir=new float[] { ldir.X, ldir.Y, ldir.Z };
                    effect.SetValue(ehLightDir, dir);
                    effect.SetValue(ehLightCol, ColorOperator.Scale(LightColor, lightScale));
                    effect.SetValue(egAmbCol, ColorOperator.Scale(AmbientLightColor, ambLightScale));
                    effect.SetValue(ehViewProj, camera.ViewMatrix * camera.ProjectionMatrix);
                    dir=new float[] { camera.EyeLocation.X, camera.EyeLocation.Y, camera.EyeLocation.Z };
                    effect.SetValue(ehEyePos, dir);
                    dir=new float[] { edir.X, edir.Y, edir.Z };
                    effect.SetValue(ehEyeVec, dir);
                    dir=new float[] { hdir.X, hdir.Y, hdir.Z };
                    effect.SetValue(ehHalfVec, dir);

                    // Apply the technique contained in the effect
                    if(currentSubset==-1) nif.Render();
                    else nif.RenderSubset(currentSubset);
                }

                // Show frame rate and help, etc
                RenderText(appTime);

                // Show UI
                hud.OnRender(elapsedTime);
                sampleUi.OnRender(elapsedTime);
            } finally {
                if(beginSceneCalled) device.EndScene();
            }
        }

        /// <summary>
        /// Render the help and statistics text. This function uses the Font object for 
        /// efficient text rendering.
        /// </summary>
        private void RenderText(double appTime) {
            TextHelper txtHelper = new TextHelper(statsFont, textSprite, 15);

            // Output statistics
            txtHelper.Begin();
            txtHelper.SetInsertionPoint(2, 0);
            txtHelper.SetForegroundColor(unchecked((int)0xffffff00));
            if(isLogShowing) {
                txtHelper.DrawTextLine(NifFile.loadLog);
            } else {
                txtHelper.DrawTextLine(sampleFramework.FrameStats);
                txtHelper.DrawTextLine(sampleFramework.DeviceStats);
            }
            

            // Draw help
            if(isHelpShowing) {
                txtHelper.SetInsertionPoint(10, sampleFramework.BackBufferSurfaceDescription.Height-15*6);
                txtHelper.SetForegroundColor(unchecked((int)0xffffffff));
                txtHelper.SetForegroundColor(System.Drawing.Color.DarkOrange);
                txtHelper.DrawTextLine("Controls (F1 to hide):");

                txtHelper.SetInsertionPoint(20, sampleFramework.BackBufferSurfaceDescription.Height-15*5);
                txtHelper.DrawTextLine("Rotate model: Left mouse button");
                txtHelper.DrawTextLine("Rotate light: Right mouse button");
                txtHelper.DrawTextLine("Rotate camera: Middle mouse button");
                txtHelper.DrawTextLine("Zoom camera: Mouse wheel scroll");

                txtHelper.SetInsertionPoint(250, sampleFramework.BackBufferSurfaceDescription.Height-15*5);
                txtHelper.DrawTextLine("Move camera: Arrow keys");
                txtHelper.DrawTextLine("Reset camera: Home");
                txtHelper.DrawTextLine("Hide help: F1");
                txtHelper.DrawTextLine("Quit: Esc");
            } else {
                txtHelper.SetInsertionPoint(10, sampleFramework.BackBufferSurfaceDescription.Height-15*2);
                txtHelper.SetForegroundColor(unchecked((int)0xffffffff));
                txtHelper.DrawTextLine("Press F1 for help");
            }

            if(nif!=null&&!isLogShowing) {
                txtHelper.SetForegroundColor(unchecked((int)0xffffffff));
                txtHelper.SetInsertionPoint(2, 15*3);
                txtHelper.DrawTextLine(nifPath);
                if(subsetDesc!=null) txtHelper.DrawTextLine(subsetDesc);
            }

            txtHelper.End();
        }

        /// <summary>
        /// As a convenience, the sample framework inspects the incoming windows messages for
        /// keystroke messages and decodes the message parameters to pass relevant keyboard
        /// messages to the application.  The framework does not remove the underlying keystroke 
        /// messages, which are still passed to the application's MsgProc callback.
        /// </summary>
        private void OnKeyEvent(object sender, System.Windows.Forms.KeyEventArgs e) {
            switch(e.KeyCode) {
            case System.Windows.Forms.Keys.F1:
                isHelpShowing = !isHelpShowing;
                break;
            case System.Windows.Forms.Keys.L:
                isLogShowing=!isLogShowing;
                break;
            }
        }

        /// <summary>
        /// Before handling window messages, the sample framework passes incoming windows 
        /// messages to the application through this callback function. If the application sets 
        /// noFurtherProcessing to true, the sample framework will not process the message
        /// </summary>
        public IntPtr OnMsgProc(IntPtr hWnd, NativeMethods.WindowMessage msg, IntPtr wParam, IntPtr lParam, ref bool noFurtherProcessing) {
            // Give the dialog a chance to handle the message first
            noFurtherProcessing = hud.MessageProc(hWnd, msg, wParam, lParam);
            if(noFurtherProcessing)
                return IntPtr.Zero;

            noFurtherProcessing = sampleUi.MessageProc(hWnd, msg, wParam, lParam);
            if(noFurtherProcessing)
                return IntPtr.Zero;

            // Give the light control a chance now
            lightControl.HandleMessages(hWnd, msg, wParam, lParam);

            // Pass all remaining windows messages to camera so it can respond to user input
            camera.HandleMessages(hWnd, msg, wParam, lParam);

            return IntPtr.Zero;
        }

        /// <summary>Initializes the application</summary>
        public void InitializeApplication() {
                lightControl = new DirectionWidget();
                lightControl.LightDirection = new Vector3((float)Math.Sin((float)Math.PI
                    * 2 -(float)Math.PI/6), 0, -(float)Math.Cos((float)Math.PI
                    * 2 -(float)Math.PI/6));

            lightScale = 1.0f;
            ambLightScale = 0.1f;

            int y = 10;
            // Initialize the dialogs
            Button fullScreen = hud.AddButton(ToggleFullscreen, "Toggle full screen", 35, y, 125, 22);
            Button changeDevice = hud.AddButton(ChangeDevice, "Change Device (F2)", 35, y += 24, 125, 22);
            // Hook the button events for when these items are clicked
            fullScreen.Click += new EventHandler(OnFullscreenClicked);
            changeDevice.Click += new EventHandler(OnChangeDevicClicked);

            // Now add the sample specific UI
            y = 10;

            //lighting
            sampleUi.AddStatic(LightScaleStatic, string.Format("Light scale: {0}", lightScale.ToString("f2",
                System.Globalization.CultureInfo.CurrentUICulture)), 35, y += 24, 125, 22);
            Slider scaleSlider = sampleUi.AddSlider(LightScaleControl, 50, y += 24, 100, 22, 0, 20, (int)(lightScale * 10.0f), false);
            Button lightButton = sampleUi.AddButton(LightColorPicker, "Change light colour", 35, y+=24, 125, 22);
            
            sampleUi.AddStatic(AmbLightScaleStatic, string.Format("Ambient light scale: {0}", ambLightScale.ToString("f2",
                System.Globalization.CultureInfo.CurrentUICulture)), 35, y += 24, 125, 22);
            Slider ambScaleSlider = sampleUi.AddSlider(AmbLightScaleControl, 50, y += 24, 100, 22, 0, 20, (int)(ambLightScale * 10.0f), false);
            Button ambLightButton = sampleUi.AddButton(AmbLightColorPicker, "Change ambient colour", 35, y+=24, 125, 22);

            y+=19;
            ComboBox shaderPicker = sampleUi.AddComboBox(ChangeShader, 35, y+=24, 125, 22);
            shaderPicker.AddItem("Standard", null);
            shaderPicker.AddItem("Color map", "ColorMap");
            shaderPicker.AddItem("Alpha/height map", "HeightMap");
            shaderPicker.AddItem("Normal map", "NormalMap");
            shaderPicker.AddItem("Specular map", "SpecularMap");
            shaderPicker.AddItem("Glow map", "GlowMap");
            shaderPicker.AddItem("Ambient material", "AmbMat");
            shaderPicker.AddItem("Diffuse material", "DifMat");
            shaderPicker.AddItem("Specular material", "SpecMat");
            shaderPicker.AddItem("Emissive material", "GlowMat");
            shaderPicker.AddItem("Vertex position", "vPosition");
            shaderPicker.AddItem("Vertex normals", "vNormal");
            shaderPicker.AddItem("Vertex colours", "vColor");
            shaderPicker.AddItem("Texture coords", "vUV");
            shaderPicker.AddItem("Vertex tangents", "vTangent");
            shaderPicker.AddItem("Vertex binormals", "vBinormal");
            Checkbox wireframeCheckbox = sampleUi.AddCheckBox(WireFrame, "Wireframe", 35, y+=24, 125, 22, false);
            Checkbox cullCheckBox = sampleUi.AddCheckBox(ToggleCulling, "Backface culling", 35, y+=24, 125, 22, true);

            y+=19;
            ComboBox subsetControl = sampleUi.AddComboBox(SubsetControl, 35, y+=24, 125, 22);

            y+=19;
            Button loadButton = sampleUi.AddButton(LoadMeshButton, "Load mesh", 35, y+=24, 125, 22);

            // Hook the events
            scaleSlider.ValueChanged += new EventHandler(OnLightScaleChanged);
            lightButton.Click+=new EventHandler(lightButton_Click);
            ambScaleSlider.ValueChanged+=new EventHandler(ambScaleSlider_ValueChanged);
            ambLightButton.Click+=new EventHandler(ambLightButton_Click);
            shaderPicker.Changed +=new EventHandler(shaderPicker_Changed);
            wireframeCheckbox.Changed+= new EventHandler(wireframeCheckbox_Changed);
            subsetControl.Changed+=new EventHandler(subsetControl_Changed);
            loadButton.Click+=new EventHandler(loadButton_Click);
            cullCheckBox.Changed+=new EventHandler(cullCheckBox_Changed);
        }

        void cullCheckBox_Changed(object sender, EventArgs e) {
            Checkbox cb=(Checkbox)sender;
            if(cb.IsChecked) Device.RenderState.CullMode=Cull.CounterClockwise;
            else Device.RenderState.CullMode=Cull.None;
        }

        void applyModePicker_Changed(object sender, EventArgs e) {
            ComboBox cb=(ComboBox)sender;
            if(nif==null) {
                if((byte)cb.GetSelectedData()!=0) MessageBox.Show("No mesh is currently loaded", "Error");
                cb.SetSelected(0);
                return;
            }
            if(currentSubset==-1) {
                if((byte)cb.GetSelectedData()!=0) MessageBox.Show("Can only modify apply mode for an individual subset", "Error");
                cb.SetSelected(0);
                return;
            }
            nif.SetApplyMode(currentSubset, (byte)cb.GetSelectedData());
        }

        void subsetControl_Changed(object sender, EventArgs e) {
            ComboBox cb=(ComboBox)sender;
            if(cb.NumberItems>0) currentSubset=(int)cb.GetSelectedData();

            if(currentSubset==-1) {
                subsetDesc=null;
            } else {
                NifFile.SubsetData data=nif.GetSubsetData(currentSubset);
                subsetDesc=data.path+Environment.NewLine+
                    "Base texture: "+data.cWidth+", "+data.cHeight+" ("+data.cformat.ToString()+")"+Environment.NewLine+
                    "Normal map: "+(data.nWidth==-1?"None found":(data.nWidth+", "+data.nHeight)+" ("+data.nformat.ToString()+")")+Environment.NewLine+
                    "Glow map: "+(data.gWidth==-1?"None found":(data.gWidth+", "+data.gHeight)+" ("+data.gformat.ToString()+")");
            }
        }

        void ambLightButton_Click(object sender, EventArgs e) {
            if(colorDialog.ShowDialog()==System.Windows.Forms.DialogResult.OK) {
                AmbientLightColor=new ColorValue((int)colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B, 1);
            }
        }

        void lightButton_Click(object sender, EventArgs e) {
            if(colorDialog.ShowDialog()==System.Windows.Forms.DialogResult.OK) {
                LightColor=new ColorValue((int)colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B, 1);
            }
        }

        void loadButton_Click(object sender, EventArgs e) {
            MeshLoader ml=new MeshLoader();
            if(ml.ShowDialog()==System.Windows.Forms.DialogResult.OK) {
                LoadNif(ml.SelectedMesh);
            }

            //LoadNif(@"meshes\dungeons\chargen\prisoncell01.nif");
            //LoadNif(@"meshes\dungeons\chargen\idpitflr2way01a.nif");
        }

        void ambScaleSlider_ValueChanged(object sender, EventArgs e) {
            Slider sl = sender as Slider;
            ambLightScale = (float)(sl.Value * 0.10f);

            StaticText text = sampleUi.GetStaticText(AmbLightScaleStatic);
            text.SetText(string.Format("Light scale: {0}", ambLightScale.ToString("f2",
                System.Globalization.CultureInfo.CurrentUICulture)));
        }

        void wireframeCheckbox_Changed(object sender, EventArgs e) {
            //sampleFramework.Device.RenderState.FillMode=((Checkbox)sender).IsChecked?FillMode.WireFrame:FillMode.Solid;
            wireframe=((Checkbox)sender).IsChecked;
        }

        void shaderPicker_Changed(object sender, EventArgs e) {
            string tech=(string)((ComboBox)sender).GetSelectedData();
            if(tech==null) {
                NifFile.SetAutoTechnique(true);
            } else {
                NifFile.SetAutoTechnique(false);
                effect.Technique=tech;
            }
        }

        void parallaxCheckbox_Changed(object sender, EventArgs e) {
            NifFile.SetParallax(((Checkbox)sender).IsChecked);
        }

        /// <summary>Called when the light scale has changed</summary>
        private void OnLightScaleChanged(object sender, EventArgs e) {
            Slider sl = sender as Slider;
            lightScale = (float)(sl.Value * 0.10f);

            StaticText text = sampleUi.GetStaticText(LightScaleStatic);
            text.SetText(string.Format("Light scale: {0}", lightScale.ToString("f2",
                System.Globalization.CultureInfo.CurrentUICulture)));
        }

        /// <summary>Called when the change device button is clicked</summary>
        private void OnChangeDevicClicked(object sender, EventArgs e) {
            sampleFramework.ShowSettingsDialog(!sampleFramework.IsD3DSettingsDialogShowing);
        }

        /// <summary>Called when the full screen button is clicked</summary>
        private void OnFullscreenClicked(object sender, EventArgs e) {
            sampleFramework.ToggleFullscreen();
        }

        /// <summary>Called when the ref button is clicked</summary>
        private void OnRefClicked(object sender, EventArgs e) {
            sampleFramework.ToggleReference();
        }

        private void LoadNif(string path) {
            nifPath=path;
            try {
                NifFile f=BSAArchive.LoadMesh(path);
                if(f==null) throw new ApplicationException("Failed to load nif: "+path);
                nif=f;
            } catch(ApplicationException ex) {
                MessageBox.Show("An error occured while loading the nif: "+ex.Message, "Error");
                return;
            }

            //Change camera radius
            lightControl.Radius = nif.Radius;
            camera.SetRadius(nif.Radius * 3.0f, 0, nif.Radius * 4.9f);
            float aspect=(float)sampleFramework.BackBufferSurfaceDescription.Width/(float)sampleFramework.BackBufferSurfaceDescription.Height;
            camera.SetProjectionParameters((float)Math.PI / 4, aspect, nif.Radius/80, nif.Radius*5);

            //Change the subset control box
            ComboBox cb=sampleUi.GetComboBox(SubsetControl);
            cb.Clear();
            cb.AddItem("Whole mesh", (int)-1);
            for(int i=0;i<nif.Subsets;i++) cb.AddItem("Subset "+i, i);
        }

        /// <summary>
        /// Entry point to the program. Initializes everything and goes into a message processing 
        /// loop. Idle time is used to render the scene.
        /// </summary>
        public static int Run(int adapter, int aa, int af, string mesh) {
#if DEBUG
            //mesh="meshes\\editorlandplane.nif";
            //mesh=@"meshes\dungeons\chargen\prisoncell01.nif";
            //mesh=@"meshes\furnituremarker01.nif";
            aa=4;
            af=16;
#endif

            AA=aa;
            AF=af;
            using(Framework sampleFramework = new Framework()) {
                BasicHLSL sample = new BasicHLSL(sampleFramework);

                sampleFramework.Disposing += new EventHandler(sample.OnDestroyDevice);
                sampleFramework.DeviceLost += new EventHandler(sample.OnLostDevice);
                sampleFramework.DeviceCreated += new DeviceEventHandler(sample.OnCreateDevice);
                sampleFramework.DeviceReset += new DeviceEventHandler(sample.OnResetDevice);
                sampleFramework.SetWndProcCallback(new WndProcCallback(sample.OnMsgProc));

                sampleFramework.SetCallbackInterface(sample);
                try {

                    // Show the cursor and clip it when in full screen
                    sampleFramework.SetCursorSettings(true, true);

                    // Initialize
                    sample.InitializeApplication();

                    sampleFramework.Initialize(true, true);
                    sampleFramework.CreateWindow("Oblivion NIF viewer");
                    sampleFramework.Window.ClientSize=new System.Drawing.Size(800, 600);
                    sampleFramework.Window.MinimumSize=sampleFramework.Window.Size;

                    sampleFramework.Window.KeyDown += new System.Windows.Forms.KeyEventHandler(sample.OnKeyEvent);
                    sampleFramework.CreateDevice(adapter, true, 800, 600, sample);

                    if(mesh!=null) {
                        sample.LoadNif(mesh);
                        if(Path.GetExtension(mesh).ToLower()!=".nif") {
                            sample.sampleUi.GetCheckbox(ToggleCulling).IsChecked=false;
                            sample.sampleUi.GetComboBox(ChangeShader).SetSelected(1);
                            sample.shaderPicker_Changed(sample.sampleUi.GetComboBox(ChangeShader), null);
                        }
                    }

                    sampleFramework.MainLoop();

                }
#if(DEBUG)
 catch(Exception e) {
                    // In debug mode show this error (maybe - depending on settings)
                    sampleFramework.DisplayErrorMessage(e);
#else
            catch
            {
                // In release mode fail silently
#endif
                    // Ignore any exceptions here, they would have been handled by other areas
                    return (sampleFramework.ExitCode == 0) ? 1 : sampleFramework.ExitCode; // Return an error code here
                }

                // Perform any application-level cleanup here. Direct3D device resources are released within the
                // appropriate callback functions and therefore don't require any cleanup code here.
                return sampleFramework.ExitCode;
            }
        }
    }
}
