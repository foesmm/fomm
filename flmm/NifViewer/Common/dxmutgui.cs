//--------------------------------------------------------------------------------------
// File: DXMUTGui.cs
//
// DirectX SDK Managed Direct3D GUI Sample Code
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
using System;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Microsoft.Samples.DirectX.UtilityToolkit
{
    /// <summary>
    /// Predefined control types
    /// </summary>
    public enum ControlType
    {
        StaticText,
        Button,
        CheckBox,
        RadioButton,
        ComboBox,
        Slider,
        ListBox,
        EditBox,
        Scrollbar,
    }

    /// <summary>
    /// Possible states of a control
    /// </summary>
    public enum ControlState
    {
        Normal,
        Disabled,
        Hidden,
        Focus,
        MouseOver,
        Pressed,
        LastState // Should always be last
    }

    /// <summary>
    /// Blends colors
    /// </summary>
    public struct BlendColor
    {
        public ColorValue[] States; // Modulate colors for all possible control states
        public ColorValue Current; // Current color

        /// <summary>Initialize the color blending</summary>
        public void Initialize(ColorValue defaultColor, ColorValue disabledColor, ColorValue hiddenColor)
        {
            // Create the array
            States = new ColorValue[(int)ControlState.LastState];
            for(int i = 0; i < States.Length; i++)
            {
                States[i] = defaultColor;
            }

            // Store the data
            States[(int)ControlState.Disabled] = disabledColor;
            States[(int)ControlState.Hidden] = hiddenColor;
            Current = hiddenColor;
        }
        /// <summary>Initialize the color blending</summary>
        public void Initialize(ColorValue defaultColor) { Initialize( defaultColor, new ColorValue(0.5f, 0.5f, 0.5f, 0.75f),new ColorValue()); }

        /// <summary>Blend the colors together</summary>
        public void Blend(ControlState state, float elapsedTime, float rate)
        {
            if ((States == null) || (States.Length == 0) )
                return; // Nothing to do

            ColorValue destColor = States[(int)state];
            Current = ColorOperator.Lerp(Current, destColor, 1.0f - (float)Math.Pow(rate, 30 * elapsedTime) );
        }
        /// <summary>Blend the colors together</summary>
        public void Blend(ControlState state, float elapsedTime) { Blend(state, elapsedTime, 0.7f); }
    }

    /// <summary>
    /// Contains all the display information for a given control type
    /// </summary>
    public struct ElementHolder
    {
        public ControlType ControlType;
        public uint ElementIndex;
        public Element Element;
    }

    /// <summary>
    /// Contains all the display tweakables for a sub-control
    /// </summary>
    public class Element : ICloneable
    {
        #region Magic Numbers
        #endregion

        #region Instance Data
        public uint TextureIndex; // Index of the texture for this Element 
        public uint FontIndex; // Index of the font for this Element 
        public DrawTextFormat textFormat; // The Format argument to draw text

        public System.Drawing.Rectangle textureRect; // Bounding rectangle of this element on the composite texture

        public BlendColor TextureColor;
        public BlendColor FontColor;
        #endregion

        /// <summary>Set the texture</summary>
        public void SetTexture(uint tex, System.Drawing.Rectangle texRect, ColorValue defaultTextureColor)
        {
            // Store data
            TextureIndex = tex;
            textureRect = texRect;
            TextureColor.Initialize(defaultTextureColor);
        }
        /// <summary>Set the texture</summary>
        public void SetTexture(uint tex, System.Drawing.Rectangle texRect) { SetTexture(tex, texRect, Dialog.WhiteColorValue); }
        /// <summary>Set the font</summary>
        public void SetFont(uint font, ColorValue defaultFontColor, DrawTextFormat format)
        {
            // Store data
            FontIndex = font;
            textFormat = format;
            FontColor.Initialize(defaultFontColor);
        }
        /// <summary>Set the font</summary>
        public void SetFont(uint font){ SetFont(font, Dialog.WhiteColorValue, DrawTextFormat.Center | DrawTextFormat.VerticalCenter ); }
        /// <summary>
        /// Refresh this element
        /// </summary>
        public void Refresh()
        {
            if (TextureColor.States != null) 
                TextureColor.Current = TextureColor.States[(int)ControlState.Hidden];
            if (FontColor.States != null) 
                FontColor.Current = FontColor.States[(int)ControlState.Hidden];
        }

        #region ICloneable Members
        /// <summary>Clone an object</summary>
        public Element Clone() 
        { 
            Element e = new Element();
            e.TextureIndex = this.TextureIndex;
            e.FontIndex = this.FontIndex;
            e.textFormat = this.textFormat;
            e.textureRect = this.textureRect; 
            e.TextureColor = this.TextureColor;
            e.FontColor = this.FontColor;

            return e;
        }
        /// <summary>Clone an object</summary>
        object ICloneable.Clone() { throw new NotSupportedException("Use the strongly typed clone.");}

        #endregion
    }


    #region Dialog Resource Manager
    /// <summary>
    /// Structure for shared textures
    /// </summary>
    public class TextureNode 
    {
        public string Filename;
        public Texture Texture;
        public uint Width;
        public uint Height;
    }

    /// <summary>
    /// Structure for shared fonts
    /// </summary>
    public class FontNode 
    {
        public string FaceName;
        public Font Font;
        public uint Height;
        public FontWeight Weight;
    }

    /// <summary>
    /// Manages shared resources of dialogs
    /// </summary>
    public sealed class DialogResourceManager
    {
        private StateBlock dialogStateBlock;  // Stateblock shared amongst all dialogs
        private Sprite dialogSprite; // Sprite used for drawing
        public StateBlock StateBlock { get { return dialogStateBlock; } }
        public Sprite Sprite { get { return dialogSprite; } }
        private Device device; // Device

        // Lists of textures/fonts
        private ArrayList textureCache = new ArrayList();
        private ArrayList fontCache = new ArrayList();

        #region Creation
        /// <summary>Do not allow creation</summary>
        private DialogResourceManager()  {
            device = null;
            dialogSprite = null;
            dialogStateBlock = null;
        } 

        private static DialogResourceManager localObject = null;
        public static DialogResourceManager GetGlobalInstance()
        {
            if (localObject == null)
                localObject = new DialogResourceManager();

            return localObject;
        }
        #endregion

        /// <summary>Gets a font node from the cache</summary>
        public FontNode GetFontNode(int index) { return fontCache[index] as FontNode; }
        /// <summary>Gets a texture node from the cache</summary>
        public TextureNode GetTextureNode(int index) { return textureCache[index] as TextureNode; }
        /// <summary>Gets the device</summary>
        public Device Device { get { return device; } }

        /// <summary>
        /// Adds a font to the resource manager
        /// </summary>
        public int AddFont(string faceName, uint height, FontWeight weight)
        {
            // See if this font exists
            for(int i = 0; i < fontCache.Count; i++)
            {
                FontNode fn = fontCache[i] as FontNode;
                if ( (string.Compare(fn.FaceName, faceName, true) == 0) &&
                    fn.Height == height &&
                    fn.Weight == weight)
                {
                    // Found it
                    return i;
                }
            }

            // Doesn't exist, add a new one and try to create it
            FontNode newNode = new FontNode();
            newNode.FaceName = faceName;
            newNode.Height = height;
            newNode.Weight = weight;
            fontCache.Add(newNode);

            int fontIndex = fontCache.Count-1;
            // If a device is available, try to create immediately
            if (device != null)
                CreateFont(fontIndex);

            return fontIndex;
        }
        /// <summary>
        /// Adds a texture to the resource manager
        /// </summary>
        public int AddTexture(string filename)
        {
            // See if this font exists
            for(int i = 0; i < textureCache.Count; i++)
            {
                TextureNode tn = textureCache[i] as TextureNode;
                if (string.Compare(tn.Filename, filename, true) == 0)
                {
                    // Found it
                    return i;
                }
            }
            // Doesn't exist, add a new one and try to create it
            TextureNode newNode = new TextureNode();
            newNode.Filename = filename;
            textureCache.Add(newNode);

            int texIndex = textureCache.Count-1;

            // If a device is available, try to create immediately
            if (device != null)
                CreateTexture(texIndex);

            return texIndex;

        }

        /// <summary>
        /// Creates a font
        /// </summary>
        public void CreateFont(int font)
        {
            // Get the font node here
            FontNode fn = GetFontNode(font);
            if (fn.Font != null)
                fn.Font.Dispose(); // Get rid of this

            // Create the new font
            fn.Font = new Font(device, (int)fn.Height, 0, fn.Weight, 1, false, CharacterSet.Default,
                Precision.Default, FontQuality.Default, PitchAndFamily.DefaultPitch | PitchAndFamily.FamilyDoNotCare,
                fn.FaceName);
        }

        /// <summary>
        /// Creates a texture
        /// </summary>
        public void CreateTexture(int tex)
        {
            // Get the texture node here
            TextureNode tn = GetTextureNode(tex);

            // Make sure there's a texture to create
            if ((tn.Filename == null) || (tn.Filename.Length == 0))
                return;

            // Find the texture
            string path = tn.Filename;

            // Create the new texture
            ImageInformation info = new ImageInformation();
            tn.Texture = TextureLoader.FromFile(device, path, D3DX.Default, D3DX.Default, D3DX.Default, Usage.None,
                Format.Unknown, Pool.Managed, (Filter)D3DX.Default, (Filter)D3DX.Default, 0, ref info);

            // Store dimensions
            tn.Width = (uint)info.Width;
            tn.Height = (uint)info.Height;

        }

        #region Device event callbacks
        /// <summary>
        /// Called when the device is created
        /// </summary>
        public void OnCreateDevice(Device d) 
        {
            // Store device
            device = d;

            // create fonts and textures
            for (int i = 0; i < fontCache.Count; i++)
                CreateFont(i);

            for (int i = 0; i < textureCache.Count; i++)
                CreateTexture(i);

            dialogSprite = new Sprite(d); // Create the sprite
        } 
        /// <summary>
        /// Called when the device is reset
        /// </summary>
        public void OnResetDevice(Device device)
        {
            foreach(FontNode fn in fontCache)
                fn.Font.OnResetDevice();

            if (dialogSprite != null)
                dialogSprite.OnResetDevice();
            
            // Create new state block
            dialogStateBlock = new StateBlock(device, StateBlockType.All);
        }

        /// <summary>
        /// Clear any resources that need to be lost
        /// </summary>
        public void OnLostDevice()
        {
            foreach(FontNode fn in fontCache)
            {
                if ( (fn.Font != null) && (!fn.Font.Disposed) )
                    fn.Font.OnLostDevice();
            }

            if (dialogSprite != null)
                dialogSprite.OnLostDevice();

            if (dialogStateBlock != null)
            {
                dialogStateBlock.Dispose();
                dialogStateBlock = null;
            }
        }
        
        /// <summary>
        /// Destroy any resources and clear the caches
        /// </summary>
        public void OnDestroyDevice()
        {
            foreach(FontNode fn in fontCache)
            {
                if (fn.Font != null)
                    fn.Font.Dispose();
            }
            
            foreach(TextureNode tn in textureCache)
            {
                if (tn.Texture != null)
                    tn.Texture.Dispose();
            }

            if (dialogSprite != null)
            {
                dialogSprite.Dispose();
                dialogSprite = null;
            }

            if (dialogStateBlock != null)
            {
                dialogStateBlock.Dispose();
                dialogStateBlock = null;
            }
        }

        #endregion
    }

    #endregion

    /// <summary>
    /// All controls must be assigned to a dialog, which handles
    /// input and rendering for the controls.
    /// </summary>
    public class Dialog
    {
        #region Static Data
        public const int WheelDelta = 120;
        public static readonly ColorValue WhiteColorValue = new ColorValue(1.0f, 1.0f, 1.0f, 1.0f); 
        public static readonly ColorValue TransparentWhite = new ColorValue(1.0f, 1.0f, 1.0f, 0.0f); 
        public static readonly ColorValue BlackColorValue = new ColorValue(0.0f, 0.0f, 0.0f, 1.0f); 
        private static Control controlFocus = null; // The control which has focus
        private static Control controlMouseOver = null; // The control which is hovered over
        private static Control controlMouseDown = null; // The control which the mouse was pressed on

        private static double timeRefresh = 0.0;
        /// <summary>Set the static refresh time</summary>
        public static void SetRefreshTime(float time) { timeRefresh = time; }
        #endregion

        #region Instance Data
        // Sample framework
        private Framework parent = null; 
        public Framework SampleFramework { get { return parent; } }

        // Vertex information
        private CustomVertex.TransformedColoredTextured[] vertices;

        // Timing
        private double timeLastRefresh;

        // Control/Elements
        private ArrayList controlList = new ArrayList();
        private ArrayList defaultElementList = new ArrayList();

        // Captions
        private bool hasCaption;
        private string caption;
        private int captionHeight;
        private Element captionElement;
        private bool isDialogMinimized;

        // Dialog information
        private int dialogX, dialogY, width, height;
        // Colors
        private ColorValue topLeftColor, topRightColor, bottomLeftColor, bottomRightColor;
        
        // Fonts/Textures
        private ArrayList textureList = new ArrayList(); // Index into texture cache
        private ArrayList fontList = new ArrayList(); // Index into font cache

        // Dialogs
        private Dialog nextDialog;
        private Dialog prevDialog;

        // User Input control
        private bool usingNonUserEvents;
        private bool usingKeyboardInput;
        private bool usingMouseInput;
        #endregion

        #region Simple Properties/Methods
        /// <summary>Is the dilaog using non user events</summary>
        public bool IsUsingNonUserEvents { get { return usingNonUserEvents; } set { usingNonUserEvents = value; } }
        /// <summary>Is the dilaog using keyboard input</summary>
        public bool IsUsingKeyboardInput { get { return usingKeyboardInput; } set { usingKeyboardInput = value; } }
        /// <summary>Is the dilaog using mouse input</summary>
        public bool IsUsingMouseInput { get { return usingMouseInput; } set { usingMouseInput = value; } }
        /// <summary>Is the dilaog minimized</summary>
        public bool IsMinimized { get { return isDialogMinimized; } set { isDialogMinimized = value; } }
        /// <summary>Called to set dialog's location</summary>
        public void SetLocation(int x, int y) { dialogX = x; dialogY = y; UpdateVertices(); }
        /// <summary>The dialog's location</summary>
        public System.Drawing.Point Location { 
            get {return new System.Drawing.Point(dialogX, dialogY); } 
            set { dialogX = value.X; dialogY = value.Y; UpdateVertices(); } 
        }

        /// <summary>Called to set dialog's size</summary>
        public void SetSize(int w, int h) { width = w; height = h; UpdateVertices();}
        /// <summary>Dialogs width</summary>
        public int Width { get { return width; } set { width = value; } }
        /// <summary>Dialogs height</summary>
        public int Height { get { return height; } set { height = value; } }
        /// <summary>Called to set dialog's caption</summary>
        public void SetCaptionText(string text) { caption = text; }
        /// <summary>The dialog's caption height</summary>
        public int CaptionHeight { get { return captionHeight; } set { captionHeight = value; } }
        /// <summary>Called to set dialog's caption enabled state</summary>
        public void SetCaptionEnabled(bool isEnabled) { hasCaption = isEnabled; }
        /// <summary>Called to set dialog's border colors</summary>
        public void SetBackgroundColors(ColorValue topLeft, ColorValue topRight, ColorValue bottomLeft, ColorValue bottomRight) 
        { 
            topLeftColor = topLeft; topRightColor = topRight; bottomLeftColor = bottomLeft; bottomRightColor = bottomRight;
            UpdateVertices();
        }
        /// <summary>Called to set dialog's border colors</summary>
        public void SetBackgroundColors(ColorValue allCorners) { SetBackgroundColors(allCorners, allCorners, allCorners, allCorners); }

        #endregion

        /// <summary>
        /// Create a new instance of the dialog class
        /// </summary>
        public Dialog(Framework sample)
        {
            parent = sample; // store this for later use
            // Initialize to default state
            dialogX = 0; dialogY = 0; width = 0; height = 0;
            hasCaption = false; isDialogMinimized = false; 
            caption = string.Empty;
            captionHeight = 18;

            topLeftColor = topRightColor = bottomLeftColor = bottomRightColor = new ColorValue();

            timeLastRefresh = 0.0f;
            
            nextDialog = this; // Only one dialog
            prevDialog = this;  // Only one dialog

            usingNonUserEvents = false;
            usingKeyboardInput = false;
            usingMouseInput = true;

            InitializeDefaultElements();
        }

        /// <summary>
        /// Initialize the default elements for this dialog
        /// </summary>
        private void InitializeDefaultElements()
        {
            SetTexture(0, "UI\\DXUTControls.dds");
            SetFont(0, "Arial", 14, FontWeight.Normal);

            //-------------------------------------
            // Element for the caption
            //-------------------------------------
            captionElement = new Element();
            captionElement.SetFont(0, WhiteColorValue, DrawTextFormat.Left | DrawTextFormat.VerticalCenter);
            captionElement.SetTexture(0, System.Drawing.Rectangle.FromLTRB(17, 269, 241, 287));
            captionElement.TextureColor.States[(int)ControlState.Normal] = WhiteColorValue;
            captionElement.FontColor.States[(int)ControlState.Normal] = WhiteColorValue;
            // Pre-blend as we don't need to transition the state
            captionElement.TextureColor.Blend(ControlState.Normal, 10.0f);
            captionElement.FontColor.Blend(ControlState.Normal, 10.0f);

            Element e = new Element();

            //-------------------------------------
            // StaticText
            //-------------------------------------
            e.SetFont(0);
            e.FontColor.States[(int)ControlState.Disabled] = new ColorValue(0.75f, 0.75f, 0.75f, 0.75f);
            // Assign the element
            SetDefaultElement(ControlType.StaticText, StaticText.TextElement, e);

            //-------------------------------------
            // Button - Button
            //-------------------------------------
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(0, 0, 136, 54));
            e.SetFont(0);
            e.TextureColor.States[(int)ControlState.Normal] = new ColorValue(1.0f, 1.0f, 1.0f, 0.55f);
            e.TextureColor.States[(int)ControlState.Pressed] = new ColorValue(1.0f, 1.0f, 1.0f, 0.85f);
            e.FontColor.States[(int)ControlState.MouseOver] = BlackColorValue;
            // Assign the element
            SetDefaultElement(ControlType.Button, Button.ButtonLayer, e);

            //-------------------------------------
            // Button - Fill Layer
            //-------------------------------------
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(136, 0, 252, 54), TransparentWhite);
            e.TextureColor.States[(int)ControlState.MouseOver] = new ColorValue(1.0f, 1.0f, 1.0f, 0.6f);
            e.TextureColor.States[(int)ControlState.Pressed] = new ColorValue(0,0,0, 0.25f);
            e.TextureColor.States[(int)ControlState.Focus] = new ColorValue(1.0f, 1.0f, 1.0f, 0.05f);
            // Assign the element
            SetDefaultElement(ControlType.Button, Button.FillLayer, e);


            //-------------------------------------
            // CheckBox - Box
            //-------------------------------------
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(0, 54, 27, 81));
            e.SetFont(0, WhiteColorValue, DrawTextFormat.Left | DrawTextFormat.VerticalCenter);
            e.FontColor.States[(int)ControlState.Disabled] = new ColorValue(0.8f, 0.8f, 0.8f, 0.8f);
            e.TextureColor.States[(int)ControlState.Normal] = new ColorValue(1.0f, 1.0f, 1.0f, 0.55f);
            e.TextureColor.States[(int)ControlState.Focus] = new ColorValue(1.0f, 1.0f, 1.0f, 0.8f);
            e.TextureColor.States[(int)ControlState.Pressed] = WhiteColorValue;
            // Assign the element
            SetDefaultElement(ControlType.CheckBox, Checkbox.BoxLayer, e);

            //-------------------------------------
            // CheckBox - Check
            //-------------------------------------
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(27, 54, 54, 81));
            // Assign the element
            SetDefaultElement(ControlType.CheckBox, Checkbox.CheckLayer, e);

            //-------------------------------------
            // RadioButton - Box
            //-------------------------------------
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(54, 54, 81, 81));
            e.SetFont(0, WhiteColorValue, DrawTextFormat.Left | DrawTextFormat.VerticalCenter);
            e.FontColor.States[(int)ControlState.Disabled] = new ColorValue(0.8f, 0.8f, 0.8f, 0.8f);
            e.TextureColor.States[(int)ControlState.Normal] = new ColorValue(1.0f, 1.0f, 1.0f, 0.55f);
            e.TextureColor.States[(int)ControlState.Focus] = new ColorValue(1.0f, 1.0f, 1.0f, 0.8f);
            e.TextureColor.States[(int)ControlState.Pressed] = WhiteColorValue;
            // Assign the element
            SetDefaultElement(ControlType.RadioButton, RadioButton.BoxLayer, e);

            //-------------------------------------
            // RadioButton - Check
            //-------------------------------------
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(81, 54, 108, 81));
            // Assign the element
            SetDefaultElement(ControlType.RadioButton, RadioButton.CheckLayer, e);

            //-------------------------------------
            // ComboBox - Main
            //-------------------------------------
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(7, 81, 247, 123));
            e.SetFont(0);
            e.TextureColor.States[(int)ControlState.Normal] = new ColorValue(0.8f, 0.8f, 0.8f, 0.55f);
            e.TextureColor.States[(int)ControlState.Focus] = new ColorValue(0.95f, 0.95f, 0.95f, 0.6f);
            e.TextureColor.States[(int)ControlState.Disabled] = new ColorValue(0.8f, 0.8f, 0.8f, 0.25f);
            e.FontColor.States[(int)ControlState.MouseOver] = new ColorValue(0,0,0,1.0f);
            e.FontColor.States[(int)ControlState.Pressed] = new ColorValue(0,0,0,1.0f);
            e.FontColor.States[(int)ControlState.Disabled] = new ColorValue(0.8f, 0.8f, 0.8f, 0.8f);
            // Assign the element
            SetDefaultElement(ControlType.ComboBox, ComboBox.MainLayer, e);

            //-------------------------------------
            // ComboBox - Button
            //-------------------------------------
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(98, 189, 151, 238));
            e.TextureColor.States[(int)ControlState.Normal] = new ColorValue(1.0f, 1.0f, 1.0f, 0.55f);
            e.TextureColor.States[(int)ControlState.Pressed] = new ColorValue(0.55f, 0.55f, 0.55f, 1.0f);
            e.TextureColor.States[(int)ControlState.Focus] = new ColorValue(1.0f, 1.0f, 1.0f, 0.75f);
            e.TextureColor.States[(int)ControlState.Disabled] = new ColorValue(1.0f, 1.0f, 1.0f, 0.25f);
            // Assign the element
            SetDefaultElement(ControlType.ComboBox, ComboBox.ComboButtonLayer, e);

            //-------------------------------------
            // ComboBox - Dropdown
            //-------------------------------------
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(13, 123, 241, 160));
            e.SetFont(0, BlackColorValue, DrawTextFormat.Left | DrawTextFormat.Top);
            // Assign the element
            SetDefaultElement(ControlType.ComboBox, ComboBox.DropdownLayer, e);

            //-------------------------------------
            // ComboBox - Selection
            //-------------------------------------
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(12, 163, 239, 183));
            e.SetFont(0, WhiteColorValue, DrawTextFormat.Left | DrawTextFormat.Top);
            // Assign the element
            SetDefaultElement(ControlType.ComboBox, ComboBox.SelectionLayer, e);

            //-------------------------------------
            // Slider - Track
            //-------------------------------------
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(1, 187, 93, 228));
            e.TextureColor.States[(int)ControlState.Normal] = new ColorValue(1.0f, 1.0f, 1.0f, 0.55f);
            e.TextureColor.States[(int)ControlState.Focus] = new ColorValue(1.0f, 1.0f, 1.0f, 0.75f);
            e.TextureColor.States[(int)ControlState.Disabled] = new ColorValue(1.0f, 1.0f, 1.0f, 0.25f);
            // Assign the element
            SetDefaultElement(ControlType.Slider, Slider.TrackLayer, e);

            //-------------------------------------
            // Slider - Button
            //-------------------------------------
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(151, 193, 192, 234));
            // Assign the element
            SetDefaultElement(ControlType.Slider, Slider.ButtonLayer, e);

            //-------------------------------------
            // Scrollbar - Track
            //-------------------------------------
            int scrollBarStartX = 196;
            int scrollBarStartY = 191;
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(scrollBarStartX + 0, scrollBarStartY + 21, scrollBarStartX + 22, scrollBarStartY + 32));
            // Assign the element
            SetDefaultElement(ControlType.Scrollbar, ScrollBar.TrackLayer, e);

            //-------------------------------------
            // Scrollbar - Up Arrow
            //-------------------------------------
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(scrollBarStartX + 0, scrollBarStartY + 1, scrollBarStartX + 22, scrollBarStartY + 21));
            e.TextureColor.States[(int)ControlState.Disabled] = new ColorValue(0.8f, 0.8f, 0.8f, 1.0f);
            // Assign the element
            SetDefaultElement(ControlType.Scrollbar, ScrollBar.UpButtonLayer, e);

            //-------------------------------------
            // Scrollbar - Down Arrow
            //-------------------------------------
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(scrollBarStartX + 0, scrollBarStartY + 32, scrollBarStartX + 22, scrollBarStartY + 53));
            e.TextureColor.States[(int)ControlState.Disabled] = new ColorValue(0.8f, 0.8f, 0.8f, 1.0f);
            // Assign the element
            SetDefaultElement(ControlType.Scrollbar, ScrollBar.DownButtonLayer, e);

            //-------------------------------------
            // Scrollbar - Button
            //-------------------------------------
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(220, 192, 238, 234));
            // Assign the element
            SetDefaultElement(ControlType.Scrollbar, ScrollBar.ThumbLayer, e);


            //-------------------------------------
            // EditBox
            //-------------------------------------
            // Element assignment:
            //   0 - text area
            //   1 - top left border
            //   2 - top border
            //   3 - top right border
            //   4 - left border
            //   5 - right border
            //   6 - lower left border
            //   7 - lower border
            //   8 - lower right border
            e.SetFont(0, BlackColorValue, DrawTextFormat.Left | DrawTextFormat.Top);
            
            // Assign the styles
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(14, 90, 241, 113));
            SetDefaultElement(ControlType.EditBox, EditBox.TextLayer, e);
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(8, 82, 14, 90));
            SetDefaultElement(ControlType.EditBox, EditBox.TopLeftBorder, e);
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(14, 82, 241, 90));
            SetDefaultElement(ControlType.EditBox, EditBox.TopBorder, e);
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(241, 82, 246, 90));
            SetDefaultElement(ControlType.EditBox, EditBox.TopRightBorder, e);
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(8, 90, 14, 113));
            SetDefaultElement(ControlType.EditBox, EditBox.LeftBorder, e);
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(241, 90, 246, 113));
            SetDefaultElement(ControlType.EditBox, EditBox.RightBorder, e);
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(8, 113, 14, 121));
            SetDefaultElement(ControlType.EditBox, EditBox.LowerLeftBorder, e);
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(14, 113, 241, 121));
            SetDefaultElement(ControlType.EditBox, EditBox.LowerBorder, e);
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(241, 113, 246, 121));
            SetDefaultElement(ControlType.EditBox, EditBox.LowerRightBorder, e);


            //-------------------------------------
            // Listbox - Main
            //-------------------------------------
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(13, 123, 241, 160));
            e.SetFont(0, BlackColorValue, DrawTextFormat.Left | DrawTextFormat.Top);
            // Assign the element
            SetDefaultElement(ControlType.ListBox, ListBox.MainLayer, e);

            //-------------------------------------
            // Listbox - Selection
            //-------------------------------------
            e.SetTexture(0, System.Drawing.Rectangle.FromLTRB(16, 166, 240, 183));
            e.SetFont(0, WhiteColorValue, DrawTextFormat.Left | DrawTextFormat.Top);
            // Assign the element
            SetDefaultElement(ControlType.ListBox, ListBox.SelectionLayer, e);
        }

        /// <summary>Removes all controls from this dialog</summary>
        public void RemoveAllControls()
        {
            controlList.Clear();
            if ( (controlFocus != null) && (controlFocus.Parent == this) )
                controlFocus = null;
            
            controlMouseOver = null;
        }

        /// <summary>Clears the radio button group</summary>
        public void ClearRadioButtonGroup(uint groupIndex)
        {
            // Find all radio buttons with the given group number
            foreach(Control c in controlList)
            {
                if (c.ControlType == ControlType.RadioButton)
                {
                    RadioButton rb = c as RadioButton;
                    // Clear the radio button checked setting
                    if (rb.ButtonGroup == groupIndex)
                        rb.SetChecked(false, false);
                }
            }
        }

        /// <summary>Clears the combo box of all items</summary>
        public void ClearComboBox(int id)
        {
            ComboBox comboBox = GetComboBox(id);
            if (comboBox == null)
                return;

            comboBox.Clear();
        }

        #region Message handling
        private static bool isDragging;
        /// <summary>
        /// Handle messages for this dialog
        /// </summary>
        public bool MessageProc(IntPtr hWnd, NativeMethods.WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            // If caption is enable, check for clicks in the caption area.
            if (hasCaption)
            {
                if (msg == NativeMethods.WindowMessage.LeftButtonDown || msg == NativeMethods.WindowMessage.LeftButtonDoubleClick)
                {
                    // Current mouse position
                    short mouseX = NativeMethods.LoWord((uint)lParam.ToInt32());
                    short mouseY = NativeMethods.HiWord((uint)lParam.ToInt32());

                    if (mouseX >= dialogX && mouseX < dialogX + width && 
                        mouseY >= dialogY && mouseY < dialogY + captionHeight)
                    {
                        isDragging = true;
                        NativeMethods.SetCapture(hWnd);
                        return true;
                    }
                }
                else if ( (msg == NativeMethods.WindowMessage.LeftButtonUp) && isDragging)
                {
                    // Current mouse position
                    short mouseX = NativeMethods.LoWord((uint)lParam.ToInt32());
                    short mouseY = NativeMethods.HiWord((uint)lParam.ToInt32());

                    if (mouseX >= dialogX && mouseX < dialogX + width && 
                        mouseY >= dialogY && mouseY < dialogY + captionHeight)
                    {
                        NativeMethods.ReleaseCapture();
                        isDragging = false;
                        return true;
                    }
                }
            }
            
            // If the dialog is minimized, don't send any messages to controls.
            if (isDialogMinimized)
                return false;

            // If a control is in focus, it belongs to this dialog, and it's enabled, then give
            // it the first chance at handling the message.
            if (controlFocus != null && 
                controlFocus.Parent == this && 
                controlFocus.IsEnabled)
            {
                // If the control MsgProc handles it, then we don't.
                if (controlFocus.MsgProc(hWnd, msg, wParam, lParam))
                    return true;
            }

            switch(msg)
            {
                // Call OnFocusIn()/OnFocusOut() of the control that currently has the focus
                // as the application is activated/deactivated.  This matches the Windows
                // behavior.
                case NativeMethods.WindowMessage.ActivateApplication:
                {
                    if (controlFocus != null && 
                        controlFocus.Parent == this && 
                        controlFocus.IsEnabled)
                    {
                        if (wParam != IntPtr.Zero)
                            controlFocus.OnFocusIn();
                        else
                            controlFocus.OnFocusOut();
                    }
                }
                    break;

                // Keyboard messages
                case NativeMethods.WindowMessage.KeyDown:
                case NativeMethods.WindowMessage.SystemKeyDown:
                case NativeMethods.WindowMessage.KeyUp:
                case NativeMethods.WindowMessage.SystemKeyUp:
                {
                    // If a control is in focus, it belongs to this dialog, and it's enabled, then give
                    // it the first chance at handling the message.
                    if (controlFocus != null && 
                        controlFocus.Parent == this && 
                        controlFocus.IsEnabled)
                    {
                        // If the control MsgProc handles it, then we don't.
                        if (controlFocus.HandleKeyboard(msg, wParam, lParam))
                            return true;
                    }

                    // Not yet handled, see if this matches a control's hotkey
                    if (msg == NativeMethods.WindowMessage.KeyUp)
                    {
                        foreach(Control c in controlList)
                        {
                            // Was the hotkey hit?
                            if (c.Hotkey == (System.Windows.Forms.Keys)wParam.ToInt32())
                            {
                                // Yup!
                                c.OnHotKey();
                                return true;
                            }
                        }
                    }
                    if (msg == NativeMethods.WindowMessage.KeyDown)
                    {
                        // If keyboard input is not enabled, this message should be ignored
                        if (!usingKeyboardInput)
                            return false;

                        System.Windows.Forms.Keys key = (System.Windows.Forms.Keys)wParam.ToInt32();
                        switch(key)
                        {
                            case System.Windows.Forms.Keys.Right:
                            case System.Windows.Forms.Keys.Down:
                                if (controlFocus != null)
                                {
                                    OnCycleFocus(true);
                                    return true;
                                }
                                break;
                            case System.Windows.Forms.Keys.Left:
                            case System.Windows.Forms.Keys.Up:
                                if (controlFocus != null)
                                {
                                    OnCycleFocus(false);
                                    return true;
                                }
                                break;
                            case System.Windows.Forms.Keys.Tab:
                                if (controlFocus == null)
                                {
                                    FocusDefaultControl();
                                }
                                else
                                {
                                    bool shiftDown = NativeMethods.IsKeyDown(System.Windows.Forms.Keys.ShiftKey);

                                    OnCycleFocus(!shiftDown);
                                }
                                return true;
                        }
                    }
                }
                    break;

                // Mouse messages
                case NativeMethods.WindowMessage.MouseMove:
                case NativeMethods.WindowMessage.MouseWheel:
                case NativeMethods.WindowMessage.LeftButtonUp:
                case NativeMethods.WindowMessage.LeftButtonDown:
                case NativeMethods.WindowMessage.LeftButtonDoubleClick:
                case NativeMethods.WindowMessage.RightButtonUp:
                case NativeMethods.WindowMessage.RightButtonDown:
                case NativeMethods.WindowMessage.RightButtonDoubleClick:
                case NativeMethods.WindowMessage.MiddleButtonUp:
                case NativeMethods.WindowMessage.MiddleButtonDown:
                case NativeMethods.WindowMessage.MiddleButtonDoubleClick:
                case NativeMethods.WindowMessage.XButtonUp:
                case NativeMethods.WindowMessage.XButtonDown:
                case NativeMethods.WindowMessage.XButtonDoubleClick:
                {
                    // If not accepting mouse input, return false to indicate the message should still 
                    // be handled by the application (usually to move the camera).
                    if (!usingMouseInput)
                        return false;

                    // Current mouse position
                    short mouseX = NativeMethods.LoWord((uint)lParam.ToInt32());
                    short mouseY = NativeMethods.HiWord((uint)lParam.ToInt32());
                    System.Drawing.Point mousePoint = new System.Drawing.Point(mouseX, mouseY);
                    // Offset mouse point
                    mousePoint.X -= dialogX;
                    mousePoint.Y -= dialogY;

                    // If caption is enabled, offset the Y coordinate by the negative of its height.
                    if (hasCaption)
                        mousePoint.Y -= captionHeight;

                    // If a control is in focus, it belongs to this dialog, and it's enabled, then give
                    // it the first chance at handling the message.
                    if (controlFocus != null && 
                        controlFocus.Parent == this && 
                        controlFocus.IsEnabled)
                    {
                        // If the control MsgProc handles it, then we don't.
                        if (controlFocus.HandleMouse(msg, mousePoint, wParam, lParam))
                            return true;
                    }

                    // Not yet handled, see if the mouse is over any controls
                    Control control = GetControlAtPoint(mousePoint);
                    if ((control != null) && (control.IsEnabled))
                    {
                        // Let the control handle the mouse if it wants (and return true if it handles it)
                        if (control.HandleMouse(msg, mousePoint, wParam, lParam))
                            return true;
                    }
                    else
                    {
                        // Mouse not over any controls in this dialog, if there was a control
                        // which had focus it just lost it
                        if (msg == NativeMethods.WindowMessage.LeftButtonDown &&
                            controlFocus != null &&
                            controlFocus.Parent == this)
                        {
                            controlFocus.OnFocusOut();
                            controlFocus = null;
                        }
                    }

                    // Still not handled, hand this off to the dialog. Return false to indicate the
                    // message should still be handled by the application (usually to move the camera).
                    switch(msg)
                    {
                        case NativeMethods.WindowMessage.MouseMove:
                            OnMouseMove(mousePoint);
                            return false;
                    }


                }
                    break;
            }

            // Didn't handle this message
            return false;
        }

        /// <summary>
        /// Handle mouse moves
        /// </summary>
        private void OnMouseMove(System.Drawing.Point pt)
        {
            // If the mouse was previously hovering over a control, it's either
            // still over the control or has left
            if (controlMouseDown != null)
            {
                // If another dialog owns this control then let that dialog handle it
                if (controlMouseDown.Parent != this )
                    return;

                // If the same control is still under the mouse, nothing needs to be done
                if (controlMouseDown.ContainsPoint(pt))
                    return;
           
                // Mouse has moved outside the control, notify the control and continue
                controlMouseDown.OnMouseExit();
                controlMouseDown = null;
            }

            // Figure out which control the mouse is over now
            Control control = GetControlAtPoint(pt);
            if (control != null)
            {
                controlMouseDown = control;
                controlMouseDown.OnMouseEnter();
            }
        }
        #endregion
        
        #region Focus
        /// <summary>
        /// Request that this control has focus
        /// </summary>
        public static void RequestFocus(Control control)
        {
            if (controlFocus == control)
                return; // Already does

            if (!control.CanHaveFocus)
                return; // Can't have focus

            if (controlFocus != null)
                controlFocus.OnFocusOut();

            // Set the control focus now
            control.OnFocusIn();
            controlFocus = control;
        }

        /// <summary>
        /// Clears focus of the dialog
        /// </summary>
        public static void ClearFocus()
        {
            if (controlFocus != null)
            {
                controlFocus.OnFocusOut();
                controlFocus = null;
            }
        }
        /// <summary>
        /// Cycles focus to the next available control
        /// </summary>
        private void OnCycleFocus(bool forward)
        {
            // This should only be handled by the dialog which owns the focused control, and 
            // only if a control currently has focus
            if (controlFocus == null || controlFocus.Parent != this )
                return;

            Control control = controlFocus;
            // Go through a bunch of controls
            for (int i = 0; i < 0xffff; i++)
            {
                control = (forward) ? GetNextControl(control) : GetPreviousControl(control);

                // If we've gone in a full circle, focus won't change
                if (control == controlFocus)
                    return;

                // If the dialog accepts keybord input and the control can have focus then
                // move focus
                if (control.Parent.IsUsingKeyboardInput && control.CanHaveFocus)
                {
                    controlFocus.OnFocusOut();
                    controlFocus = control;
                    controlFocus.OnFocusIn();
                    return;
                }
            }

            throw new InvalidOperationException("Multiple dialogs are improperly chained together.");
        }

        /// <summary>
        /// Gets the next control
        /// </summary>
        private static Control GetNextControl(Control control)
        {
            int index = (int)control.index + 1;

            Dialog dialog = control.Parent;
            
            // Cycle through dialogs in the loop to find the next control. Note
            // that if only one control exists in all looped dialogs it will
            // be the returned 'next' control.
            while (index >= (int)dialog.controlList.Count)
            {
                dialog = dialog.nextDialog;
                index = 0;
            }

            return dialog.controlList[index] as Control;
        }
        /// <summary>
        /// Gets the previous control
        /// </summary>
        private static Control GetPreviousControl(Control control)
        {
            int index = (int)control.index - 1;

            Dialog dialog = control.Parent;
            
            // Cycle through dialogs in the loop to find the next control. Note
            // that if only one control exists in all looped dialogs it will
            // be the returned 'previous' control.
            while (index < 0)
            {
                dialog = dialog.prevDialog;
                if (dialog == null)
                    dialog = control.Parent;

                index = dialog.controlList.Count - 1;
            }

            return dialog.controlList[index] as Control;
        }
        /// <summary>
        /// Sets focus to the default control of a dialog
        /// </summary>
        private void FocusDefaultControl()
        {
            // Check for a default control in this dialog
            foreach(Control c in controlList)
            {
                if (c.isDefault)
                {
                    // Remove focus from the current control
                    ClearFocus();

                    // Give focus to the default control
                    controlFocus = c;
                    controlFocus.OnFocusIn();
                    return;
                }
            }
        }
        #endregion

        #region Controls Methods/Properties
        /// <summary>Sets the control enabled property</summary>
        public void SetControlEnable(int id, bool isenabled)
        {
            Control c = GetControl(id);
            if (c == null)
                return; // No control to set

            c.IsEnabled = isenabled;
        }
        /// <summary>Gets the control enabled property</summary>
        public bool GetControlEnable(int id)
        {
            Control c = GetControl(id);
            if (c == null)
                return false; // No control to get

            return c.IsEnabled;
        }

        /// <summary>Returns the control located at a point (if one exists)</summary>
        public Control GetControlAtPoint(System.Drawing.Point pt)
        {
            foreach(Control c in controlList)
            {
                if (c == null)
                    continue;

                if (c.IsEnabled && c.IsVisible && c.ContainsPoint(pt))
                    return c;
            }

            return null;
        }
        /// <summary>Returns the control located at this index(if one exists)</summary>
        public Control GetControl(int id)
        {
            foreach(Control c in controlList)
            {
                if (c == null)
                    continue;

                if (c.ID == id)
                    return c;
            }

            return null;
        }
        /// <summary>Returns the control located at this index of this type(if one exists)</summary>
        public Control GetControl(int id, ControlType typeControl)
        {
            foreach(Control c in controlList)
            {
                if (c == null)
                    continue;

                if ((c.ID == id) && (c.ControlType == typeControl))
                    return c;
            }

            return null;
        }

        /// <summary>Returns the static text control located at this index(if one exists)</summary>
        public StaticText GetStaticText(int id) { return GetControl(id, ControlType.StaticText) as StaticText; }
        /// <summary>Returns the button control located at this index(if one exists)</summary>
        public Button GetButton(int id) { return GetControl(id, ControlType.Button) as Button; }
        /// <summary>Returns the checkbox control located at this index(if one exists)</summary>
        public Checkbox GetCheckbox(int id) { return GetControl(id, ControlType.CheckBox) as Checkbox; }
        /// <summary>Returns the radio button control located at this index(if one exists)</summary>
        public RadioButton GetRadioButton(int id) { return GetControl(id, ControlType.RadioButton) as RadioButton; }
        /// <summary>Returns the combo box control located at this index(if one exists)</summary>
        public ComboBox GetComboBox(int id) { return GetControl(id, ControlType.ComboBox) as ComboBox; }
        /// <summary>Returns the slider control located at this index(if one exists)</summary>
        public Slider GetSlider(int id) { return GetControl(id, ControlType.Slider) as Slider; }
        /// <summary>Returns the listbox control located at this index(if one exists)</summary>
        public ListBox GetListBox(int id) { return GetControl(id, ControlType.ListBox) as ListBox; }

        public EditBox GetEditBox(int id) { return GetControl(id, ControlType.EditBox) as EditBox; }
        #endregion

        #region Default Elements
        /// <summary>
        /// Sets the default element
        /// </summary>
        public void SetDefaultElement(ControlType ctype, uint index, Element e)
        {
            // If this element already exists, just update it
            for (int i = 0; i < defaultElementList.Count; i++)
            {
                ElementHolder holder = (ElementHolder)defaultElementList[i];
                if ( (holder.ControlType == ctype) &&
                    (holder.ElementIndex == index) )
                {
                    // Found it, update it
                    holder.Element = e.Clone();
                    defaultElementList[i] = holder;
                    return;
                }
            }

            // Couldn't find it, add a new entry
            ElementHolder newEntry = new ElementHolder();
            newEntry.ControlType = ctype;
            newEntry.ElementIndex = index;
            newEntry.Element = e.Clone();

            // Add it now
            defaultElementList.Add(newEntry);
        }
        /// <summary>
        /// Gets the default element
        /// </summary>
        public Element GetDefaultElement(ControlType ctype, uint index)
        {
            for (int i = 0; i < defaultElementList.Count; i++)
            {
                ElementHolder holder = (ElementHolder)defaultElementList[i];
                if ( (holder.ControlType == ctype) &&
                    (holder.ElementIndex == index) )
                {
                    // Found it, return it
                    return holder.Element;
                }
            }
            return null;
        }
        #endregion

        #region Texture/Font Resources
        /// <summary>
        /// Shared resource access. Indexed fonts and textures are shared among
        /// all the controls.
        /// </summary>
        public void SetFont(uint index, string faceName, uint height, FontWeight weight)
        {
            // Make sure the list is at least big enough to hold this index
            for (uint i = (uint)fontList.Count; i <= index; i++)
                fontList.Add((int)(-1));

            int fontIndex = DialogResourceManager.GetGlobalInstance().AddFont(faceName, height, weight);
            fontList[(int)index] = fontIndex;
        }
        /// <summary>
        /// Shared resource access. Indexed fonts and textures are shared among
        /// all the controls.
        /// </summary>
        public FontNode GetFont(uint index)
        {
            return DialogResourceManager.GetGlobalInstance().GetFontNode((int)fontList[(int)index]);
        }
        /// <summary>
        /// Shared resource access. Indexed fonts and textures are shared among
        /// all the controls.
        /// </summary>
        public void SetTexture(uint index, string filename)
        {
            // Make sure the list is at least big enough to hold this index
            for (uint i = (uint)textureList.Count; i <= index; i++)
                textureList.Add((int)(-1));

            int textureIndex = DialogResourceManager.GetGlobalInstance().AddTexture(filename);
            textureList[(int)index] = textureIndex;
        }
        /// <summary>
        /// Shared resource access. Indexed fonts and textures are shared among
        /// all the controls.
        /// </summary>
        public TextureNode GetTexture(uint index)
        {
            return DialogResourceManager.GetGlobalInstance().GetTextureNode((int)textureList[(int)index]);
        }
        #endregion

        #region Control Creation
        /// <summary>
        /// Initializes a control
        /// </summary>
        public void InitializeControl(Control control)
        {
            if (control == null)
                throw new ArgumentNullException("control", "You cannot pass in a null control to initialize");

            // Set the index
            control.index = (uint)controlList.Count;

            // Look for a default element entires
            for (int i = 0; i < defaultElementList.Count; i++)
            {
                // Find any elements for this control
                ElementHolder holder = (ElementHolder)defaultElementList[i];
                if (holder.ControlType == control.ControlType)
                    control[holder.ElementIndex] = holder.Element;
            }

            // Initialize the control
            control.OnInitialize();
        }
        /// <summary>
        /// Adds a control to the dialog
        /// </summary>
        public void AddControl(Control control)
        {
            // Initialize the control first
            InitializeControl(control);

            // Add this to the control list
            controlList.Add(control);
        }
        /// <summary>Adds a static text control to the dialog</summary>
        public StaticText AddStatic(int id, string text, int x, int y, int w, int h, bool isDefault)
        {
            // First create the static
            StaticText s = new StaticText(this);

            // Now call the add control method
            AddControl(s);

            // Set the properties of the static now
            s.ID = id;
            s.SetText(text);
            s.SetLocation(x, y);
            s.SetSize(w,h);
            s.isDefault = isDefault;

            return s;
        }
        /// <summary>Adds a static text control to the dialog</summary>
        public StaticText AddStatic(int id, string text, int x, int y, int w, int h){return AddStatic(id, text, x, y, w, h, false); }
        /// <summary>Adds a button control to the dialog</summary>
        public Button AddButton(int id, string text, int x, int y, int w, int h, System.Windows.Forms.Keys hotkey, bool isDefault)
        {
            // First create the button
            Button b = new Button(this);

            // Now call the add control method
            AddControl(b);

            // Set the properties of the button now
            b.ID = id;
            b.SetText(text);
            b.SetLocation(x, y);
            b.SetSize(w,h);
            b.Hotkey = hotkey;
            b.isDefault = isDefault;

            return b;
        }
        /// <summary>Adds a button control to the dialog</summary>
        public Button AddButton(int id, string text, int x, int y, int w, int h) { return AddButton(id, text, x, y, w, h, 0, false); }
        /// <summary>Adds a checkbox to the dialog</summary>
        public Checkbox AddCheckBox(int id, string text, int x, int y, int w, int h, bool ischecked, System.Windows.Forms.Keys hotkey, bool isDefault)
        {
            // First create the checkbox
            Checkbox c = new Checkbox(this);

            // Now call the add control method
            AddControl(c);

            // Set the properties of the button now
            c.ID = id;
            c.SetText(text);
            c.SetLocation(x, y);
            c.SetSize(w,h);
            c.Hotkey = hotkey;
            c.isDefault = isDefault;
            c.IsChecked = ischecked;

            return c;
        }
        /// <summary>Adds a checkbox control to the dialog</summary>
        public Checkbox AddCheckBox(int id, string text, int x, int y, int w, int h, bool ischecked) { return AddCheckBox(id, text, x, y, w, h, ischecked, 0, false); }
        /// <summary>Adds a radiobutton to the dialog</summary>
        public RadioButton AddRadioButton(int id, uint groupId, string text, int x, int y, int w, int h, bool ischecked, System.Windows.Forms.Keys hotkey, bool isDefault)
        {
            // First create the RadioButton
            RadioButton c = new RadioButton(this);

            // Now call the add control method
            AddControl(c);

            // Set the properties of the button now
            c.ID = id;
            c.ButtonGroup = groupId;
            c.SetText(text);
            c.SetLocation(x, y);
            c.SetSize(w,h);
            c.Hotkey = hotkey;
            c.isDefault = isDefault;
            c.IsChecked = ischecked;

            return c;
        }
        /// <summary>Adds a radio button control to the dialog</summary>
        public RadioButton AddRadioButton(int id, uint groupId, string text, int x, int y, int w, int h, bool ischecked) { return AddRadioButton(id, groupId, text, x, y, w, h, ischecked, 0, false); }
        /// <summary>Adds a combobox control to the dialog</summary>
        public ComboBox AddComboBox(int id, int x, int y, int w, int h, System.Windows.Forms.Keys hotkey, bool isDefault)
        {
            // First create the combo
            ComboBox c = new ComboBox(this);

            // Now call the add control method
            AddControl(c);

            // Set the properties of the button now
            c.ID = id;
            c.SetLocation(x, y);
            c.SetSize(w,h);
            c.Hotkey = hotkey;
            c.isDefault = isDefault;

            return c;
        }
        /// <summary>Adds a combobox control to the dialog</summary>
        public ComboBox AddComboBox(int id, int x, int y, int w, int h) { return AddComboBox(id, x, y, w, h, 0, false); }
        /// <summary>Adds a slider control to the dialog</summary>
        public Slider AddSlider(int id, int x, int y, int w, int h, int min, int max, int initialValue, bool isDefault)
        {
            // First create the slider
            Slider c = new Slider(this);

            // Now call the add control method
            AddControl(c);

            // Set the properties of the button now
            c.ID = id;
            c.SetLocation(x, y);
            c.SetSize(w,h);
            c.isDefault = isDefault;
            c.SetRange(min, max);
            c.Value = initialValue;

            return c;
        }
        /// <summary>Adds a slider control to the dialog</summary>
        public Slider AddSlider(int id, int x, int y, int w, int h) { return AddSlider(id, x, y, w, h, 0,100,50, false); }
        /// <summary>Adds a listbox control to the dialog</summary>
        public ListBox AddListBox(int id, int x, int y, int w, int h, ListBoxStyle style)
        {
            // First create the listbox
            ListBox c = new ListBox(this);

            // Now call the add control method
            AddControl(c);

            // Set the properties of the button now
            c.ID = id;
            c.SetLocation(x, y);
            c.SetSize(w,h);
            c.Style = style;

            return c;
        }
        /// <summary>Adds a listbox control to the dialog</summary>
        public ListBox AddListBox(int id, int x, int y, int w, int h) { return AddListBox(id, x, y, w, h, ListBoxStyle.SingleSelection); }
        /// <summary>Adds an edit box control to the dialog</summary>
        public EditBox AddEditBox(int id, string text, int x, int y, int w, int h, bool isDefault)
        {
            // First create the editbox
            EditBox c = new EditBox(this);

            // Now call the add control method
            AddControl(c);

            // Set the properties of the static now
            c.ID = id;
            c.Text = (text != null) ? text : string.Empty;
            c.SetLocation(x, y);
            c.SetSize(w,h);
            c.isDefault = isDefault;

            return c;
        }
        /// <summary>Adds an edit box control to the dialog</summary>
        public EditBox AddEditBox(int id, string text, int x, int y, int w, int h){return AddEditBox(id, text, x, y, w, h, false); }
        #endregion

        /// <summary>Render the dialog</summary>
        private void UpdateVertices()
        {
            vertices = new Microsoft.DirectX.Direct3D.CustomVertex.TransformedColoredTextured[] {
                new CustomVertex.TransformedColoredTextured(dialogX,dialogY,0.5f,1.0f,topLeftColor.ToArgb(),0.0f,0.5f),
                new CustomVertex.TransformedColoredTextured(dialogX + width,dialogY,0.5f,1.0f,topRightColor.ToArgb(),1.0f,0.5f),
                new CustomVertex.TransformedColoredTextured(dialogX+width,dialogY+height,0.5f,1.0f,bottomRightColor.ToArgb(),1.0f,1.0f),
                new CustomVertex.TransformedColoredTextured(dialogX,dialogY+height,0.5f,1.0f,bottomLeftColor.ToArgb(),0.0f,1.0f)
            };
        }
        #region Drawing methods
        /// <summary>Render the dialog</summary>
        public void OnRender(float elapsedTime)
        {
            // See if the dialog needs to be refreshed
            if (timeLastRefresh < timeRefresh)
            {
                timeLastRefresh = FrameworkTimer.GetTime();
                Refresh();
            }

            Device device = DialogResourceManager.GetGlobalInstance().Device;

            // Set up a state block here and restore it when finished drawing all the controls
            DialogResourceManager.GetGlobalInstance().StateBlock.Capture();

            // Set some render/texture states
            device.RenderState.AlphaBlendEnable = true;
            device.RenderState.SourceBlend = Blend.SourceAlpha;
            device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
            device.RenderState.AlphaTestEnable = false;
            device.TextureState[0].ColorOperation = TextureOperation.SelectArg2;
            device.TextureState[0].ColorArgument2 = TextureArgument.Diffuse;
            device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;
            device.TextureState[0].AlphaArgument1 = TextureArgument.Diffuse;
            device.RenderState.ZBufferEnable = false;
            // Clear vertex/pixel shader
            device.VertexShader = null;
            device.PixelShader = null;

            // Render if not minimized
            if (!isDialogMinimized)
            {
                device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
                device.DrawUserPrimitives(PrimitiveType.TriangleFan, 2, vertices);
            }

            // Reset states
            device.TextureState[0].ColorOperation = TextureOperation.Modulate;
            device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
            device.TextureState[0].ColorArgument2 = TextureArgument.Diffuse;
            device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
            device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
            device.TextureState[0].AlphaArgument2 = TextureArgument.Diffuse;

            device.SamplerState[0].MinFilter = TextureFilter.Linear;

            // Set the texture up, and begin the sprite
            TextureNode tNode = GetTexture(0);
            device.SetTexture(0, tNode.Texture);
            DialogResourceManager.GetGlobalInstance().Sprite.Begin(SpriteFlags.DoNotSaveState);

            // Render the caption if it's enabled.
            if (hasCaption)
            {
                // DrawSprite will offset the rect down by
                // captionHeight, so adjust the rect higher
                // here to negate the effect.
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0,-captionHeight,width,0);
                DrawSprite(captionElement, rect);
                rect.Offset(5, 0); // Make a left margin
                string output = caption + ((isDialogMinimized) ? " (Minimized)" : null);
                DrawText(output, captionElement, rect, true);
            }

            // If the dialog is minimized, skip rendering
            // its controls.
            if (!isDialogMinimized)
            {
                for(int i = 0; i < controlList.Count; i++)
                {
                    // Focused control is drawn last
                    if (controlList[i] == controlFocus)
                        continue;

                    (controlList[i] as Control).Render(device, elapsedTime);
                }

                // Render the focus control if necessary
                if (controlFocus != null && controlFocus.Parent == this)
                    controlFocus.Render(device, elapsedTime);
            }

            // End the sprite and apply the stateblock
            DialogResourceManager.GetGlobalInstance().Sprite.End();
            DialogResourceManager.GetGlobalInstance().StateBlock.Apply();
        }

        /// <summary>
        /// Refresh the dialog
        /// </summary>
        private void Refresh()
        {
            // Reset the controls
            if (controlFocus != null)
                controlFocus.OnFocusOut();

            if (controlMouseOver != null)
                controlMouseOver.OnMouseExit();

            controlFocus = null;
            controlMouseDown = null;
            controlMouseOver = null;

            // Refresh any controls
            foreach(Control c in controlList)
            {
                c.Refresh();
            }

            if (usingKeyboardInput)
                FocusDefaultControl();
        }

        /// <summary>Draw's some text</summary>
        public void DrawText(string text, Element element, System.Drawing.Rectangle rect, bool shadow)
        {
            // No need to draw fully transparant layers
            if (element.FontColor.Current.Alpha == 0)
                return; // Nothing to do

            System.Drawing.Rectangle screenRect = rect;
            screenRect.Offset(dialogX, dialogY);

            // If caption is enabled, offset the Y position by its height.
            if (hasCaption)
                screenRect.Offset(0, captionHeight);

            // Set the identity transform
            DialogResourceManager.GetGlobalInstance().Sprite.Transform = Matrix.Identity;

            // Get the font node here
            FontNode fNode = GetFont(element.FontIndex);
            if (shadow)
            {
                // Render the text shadowed
                System.Drawing.Rectangle shadowRect = screenRect;
                shadowRect.Offset(1, 1);
                fNode.Font.DrawText(DialogResourceManager.GetGlobalInstance().Sprite, text,
                    shadowRect, element.textFormat, unchecked((int)0xff000000));
            }

            fNode.Font.DrawText(DialogResourceManager.GetGlobalInstance().Sprite, text,
                screenRect, element.textFormat, element.FontColor.Current.ToArgb());
        }
        /// <summary>Draw a sprite</summary>
        public void DrawSprite(Element element, System.Drawing.Rectangle rect)
        {
            // No need to draw fully transparant layers
            if (element.TextureColor.Current.Alpha == 0)
                return; // Nothing to do

            System.Drawing.Rectangle texRect = element.textureRect;
            System.Drawing.Rectangle screenRect = rect;
            screenRect.Offset(dialogX, dialogY);

            // If caption is enabled, offset the Y position by its height.
            if (hasCaption)
                screenRect.Offset(0, captionHeight);

            // Get the texture
            TextureNode tNode = GetTexture(element.TextureIndex);
            float scaleX = (float)screenRect.Width / (float)texRect.Width;
            float scaleY = (float)screenRect.Height / (float)texRect.Height;

            // Set the scaling transform
            DialogResourceManager.GetGlobalInstance().Sprite.Transform = Matrix.Scaling(scaleX, scaleY, 1.0f);
            
            // Calculate the position
            Vector3 pos = new Vector3(screenRect.Left, screenRect.Top, 0.0f);
            pos.X /= scaleX;
            pos.Y /= scaleY;

            // Finally draw the sprite
            DialogResourceManager.GetGlobalInstance().Sprite.Draw(tNode.Texture, texRect, new Vector3(), pos, element.TextureColor.Current.ToArgb()); 
        }
        /// <summary>Draw's some text</summary>
        public void DrawText(string text, Element element, System.Drawing.Rectangle rect) { this.DrawText(text, element, rect, false); }
        /// <summary>Draw a rectangle</summary>
        public void DrawRectangle(System.Drawing.Rectangle rect, ColorValue color)
        {
            // Offset the rectangle
            rect.Offset(dialogX, dialogY);

            // If caption is enabled, offset the Y position by its height
            if (hasCaption)
                rect.Offset(0, captionHeight);

            // Get the integer value of the color
            int realColor = color.ToArgb();
            // Create some vertices
            CustomVertex.TransformedColoredTextured[] verts = {
                new CustomVertex.TransformedColoredTextured((float)rect.Left - 0.5f, (float)rect.Top -0.5f, 0.5f, 1.0f, realColor, 0, 0),
                new CustomVertex.TransformedColoredTextured((float)rect.Right - 0.5f, (float)rect.Top -0.5f, 0.5f, 1.0f, realColor, 0, 0),
                new CustomVertex.TransformedColoredTextured((float)rect.Right - 0.5f, (float)rect.Bottom -0.5f, 0.5f, 1.0f, realColor, 0, 0),
                new CustomVertex.TransformedColoredTextured((float)rect.Left - 0.5f, (float)rect.Bottom -0.5f, 0.5f, 1.0f, realColor, 0, 0),
            };

            // Get the device
            Device device = SampleFramework.Device;

            // Since we're doing our own drawing here, we need to flush the sprites
            DialogResourceManager.GetGlobalInstance().Sprite.Flush();
            // Preserve the devices current vertex declaration
            using (VertexDeclaration decl = device.VertexDeclaration)
            {
                // Set the vertex format
                device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;

                // Set some texture states
                device.TextureState[0].ColorOperation = TextureOperation.SelectArg2;
                device.TextureState[0].AlphaOperation = TextureOperation.SelectArg2;

                // Draw the rectangle
                device.DrawUserPrimitives(PrimitiveType.TriangleFan, 2, verts);

                // Reset some texture states
                device.TextureState[0].ColorOperation = TextureOperation.Modulate;
                device.TextureState[0].AlphaOperation = TextureOperation.Modulate;

                // Restore the vertex declaration
                device.VertexDeclaration = decl;
            }
        }
        #endregion

    }

    #region Abstract Control class
    /// <summary>Base class for all controls</summary>
    public abstract class Control
    {
        #region Instance data
        protected Dialog parentDialog; // Parent container
        public uint index; // Index within the control list
        public bool isDefault;

        // Protected members
        protected object localUserData; // User specificied data
        protected bool visible;
        protected bool isMouseOver;
        protected bool hasFocus;
        protected int controlId; // ID Number
        protected ControlType controlType; // Control type, set in constructor
        protected System.Windows.Forms.Keys hotKey; // Controls hotkey
        protected bool enabled; // Enabled/disabled flag
        protected System.Drawing.Rectangle boundingBox; // Rectangle defining the active region of the control

        protected int controlX,controlY,width,height; // Size, scale, and positioning members

        protected ArrayList elementList = new ArrayList(); // All display elements
        #endregion

        /// <summary>Initialize the control</summary>
        public virtual void OnInitialize() {} // Nothing to do here
        /// <summary>Render the control</summary>
        public virtual void Render(Device device, float elapsedTime) {} // Nothing to do here
        /// <summary>Message Handler</summary>
        public virtual bool MsgProc(IntPtr hWnd, NativeMethods.WindowMessage msg, IntPtr wParam, IntPtr lParam) {return false;} // Nothing to do here
        /// <summary>Handle the keyboard data</summary>
        public virtual bool HandleKeyboard(NativeMethods.WindowMessage msg, IntPtr wParam, IntPtr lParam) {return false;} // Nothing to do here
        /// <summary>Handle the mouse data</summary>
        public virtual bool HandleMouse(NativeMethods.WindowMessage msg, System.Drawing.Point pt, IntPtr wParam, IntPtr lParam) {return false;} // Nothing to do here

        /// <summary>User specified data</summary>
        public object UserData { get { return localUserData; } set { localUserData = value; } }
        /// <summary>The parent dialog of this control</summary>
        public Dialog Parent { get { return parentDialog; } }
        /// <summary>Can the control have focus</summary>
        public virtual bool CanHaveFocus { get { return false; } }
        /// <summary>Called when control gets focus</summary>
        public virtual void OnFocusIn() { hasFocus = true;}
        /// <summary>Called when control loses focus</summary>
        public virtual void OnFocusOut() { hasFocus = false;}
        /// <summary>Called when mouse goes over the control</summary>
        public virtual void OnMouseEnter() { isMouseOver = true;}
        /// <summary>Called when mouse leaves the control</summary>
        public virtual void OnMouseExit() { isMouseOver = false;}
        /// <summary>Called when the control's hotkey is hit</summary>
        public virtual void OnHotKey() {} // Nothing to do here
        /// <summary>Does the control contain this point</summary>
        public virtual bool ContainsPoint(System.Drawing.Point pt) { return boundingBox.Contains(pt); }
        /// <summary>Is the control enabled</summary>
        public virtual bool IsEnabled { get { return enabled; } set { enabled = value; } }
        /// <summary>Is the control visible</summary>
        public virtual bool IsVisible { get { return visible; } set { visible = value; } }
        /// <summary>Type of the control</summary>
        public virtual ControlType ControlType { get { return controlType; } }
        /// <summary>Unique ID of the control</summary>
        public virtual int ID { get { return controlId; } set { controlId = value; } }
        /// <summary>Called to set control's location</summary>
        public virtual void SetLocation(int x, int y) { controlX = x; controlY = y; UpdateRectangles(); }
        /// <summary>Called to set control's size</summary>
        public virtual void SetSize(int w, int h) { width = w; height = h; UpdateRectangles(); }
        /// <summary>The controls hotkey</summary>
        public virtual System.Windows.Forms.Keys Hotkey { get { return hotKey; } set { hotKey = value; } }

        /// <summary>
        /// Index for the elements this control has access to
        /// </summary>
        public Element this[uint index]
        {
            get { return elementList[(int)index] as Element; }
            set 
            { 
                if (value == null)
                    throw new ArgumentNullException("ControlIndexer", "You cannot set a null element.");
                
                // Is the collection big enough?
                for(uint i = (uint)elementList.Count; i <= index; i++)
                {
                    // Add a new one
                    elementList.Add(new Element());
                }
                // Update the data (with a clone)
                elementList[(int)index] = value.Clone();
            }
        }
        /// <summary>
        /// Create a new instance of a control
        /// </summary>
        protected Control(Dialog parent)
        {
            controlType = ControlType.Button;
            parentDialog = parent;
            controlId = 0;
            index = 0;

            enabled = true;
            visible = true;
            isMouseOver = false;
            hasFocus = false;
            isDefault = false;

            controlX = 0; controlY = 0; width = 0; height = 0;
        }

        /// <summary>
        /// Refreshes the control
        /// </summary>
        public virtual void Refresh()
        {
            isMouseOver = false;
            hasFocus = false;
            for(int i = 0; i < elementList.Count; i++)
            {
                (elementList[i] as Element).Refresh();
            }
        }

        /// <summary>
        /// Updates the rectangles
        /// </summary>
        protected virtual void UpdateRectangles()
        {
            boundingBox = new System.Drawing.Rectangle(controlX, controlY, width, height);
        }
    }
    #endregion

    #region StaticText control
    /// <summary>
    /// StaticText text control
    /// </summary>
    public class StaticText : Control
    {
        public const int TextElement = 0;
        protected string textData; // Window text

        /// <summary>
        /// Create a new instance of a static text control
        /// </summary>
        public StaticText(Dialog parent) : base(parent)
        {
            controlType = ControlType.StaticText;
            parentDialog = parent;
            textData = string.Empty;
            elementList.Clear();
        }

        /// <summary>
        /// Render this control
        /// </summary>
        public override void Render(Device device, float elapsedTime)
        {
            if (!IsVisible)
                return; // Nothing to do here

            ControlState state = ControlState.Normal;
            if (!IsEnabled)
                state = ControlState.Disabled;

            // Blend the element colors
            Element e = elementList[TextElement] as Element;
            e.FontColor.Blend(state, elapsedTime);

            // Render with a shadow
            parentDialog.DrawText(textData, e, boundingBox, true);
        }

        /// <summary>
        /// Return a copy of the string
        /// </summary>
        public string GetTextCopy()
        {
            return string.Copy(textData);
        }

        /// <summary>
        /// Sets the updated text for this control
        /// </summary>
        public void SetText(string newText)
        {
            textData = newText;
        }

    }
    #endregion

    #region Button control

    /// <summary>
    /// Button control
    /// </summary>
    public class Button : StaticText
    {
        public const int ButtonLayer = 0;
        public const int FillLayer = 1;
        protected bool isPressed;
        #region Event code
        public event EventHandler Click;
        /// <summary>Create new button instance</summary>
        protected void RaiseClickEvent(Button sender, bool wasTriggeredByUser)
        {
            // Discard events triggered programatically if these types of events haven't been
            // enabled
            if (!Parent.IsUsingNonUserEvents && !wasTriggeredByUser)
                return;

            if (Click != null)
                Click(sender, EventArgs.Empty);
        }
        #endregion

        /// <summary>Create new button instance</summary>
        public Button(Dialog parent) : base(parent)
        {
            controlType = ControlType.Button;
            parentDialog = parent;
            isPressed = false;
            hotKey = 0;
        }

        /// <summary>Can the button have focus</summary>
        public override bool CanHaveFocus { get { return IsVisible && IsEnabled; } }
        /// <summary>The hotkey for this button was pressed</summary>
        public override void OnHotKey()
        {
            RaiseClickEvent(this, true);
        }

        /// <summary>
        /// Will handle the keyboard strokes
        /// </summary>
        public override bool HandleKeyboard(NativeMethods.WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false;

            switch(msg)
            {
                case NativeMethods.WindowMessage.KeyDown:
                    if ((System.Windows.Forms.Keys)wParam.ToInt32() == System.Windows.Forms.Keys.Space)
                    {
                        isPressed = true;
                        return true;
                    }
                    break;
                case NativeMethods.WindowMessage.KeyUp:
                    if ((System.Windows.Forms.Keys)wParam.ToInt32() == System.Windows.Forms.Keys.Space)
                    {
                        isPressed = false;
                        RaiseClickEvent(this, true);

                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Handle mouse messages from the buttons
        /// </summary>
        public override bool HandleMouse(NativeMethods.WindowMessage msg, System.Drawing.Point pt, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false;

            switch(msg)
            {
                case NativeMethods.WindowMessage.LeftButtonDoubleClick:
                case NativeMethods.WindowMessage.LeftButtonDown:
                {
                    if (ContainsPoint(pt))
                    {
                        // Pressed while inside the control
                        isPressed = true;
                        Parent.SampleFramework.Window.Capture = true;
                        if (!hasFocus)
                            Dialog.RequestFocus(this);

                        return true;
                    }
                }
                    break;
                case NativeMethods.WindowMessage.LeftButtonUp:
                {
                    if (isPressed)
                    {
                        isPressed = false;
                        Parent.SampleFramework.Window.Capture = false;
                        if (!parentDialog.IsUsingKeyboardInput)
                            Dialog.ClearFocus();

                        // Button click
                        if (ContainsPoint(pt))
                            RaiseClickEvent(this, true);
                    }
                }
                    break;
            }

            return false;
        }

        /// <summary>Render the button</summary>
        public override void Render(Device device, float elapsedTime)
        {
            int offsetX = 0;
            int offsetY = 0;

            ControlState state = ControlState.Normal;
            if (IsVisible == false)
            {
                state = ControlState.Hidden;
            }
            else if (IsEnabled == false)
            {
                state = ControlState.Disabled;
            }
            else if (isPressed)
            {
                state = ControlState.Pressed;
                offsetX = 1;
                offsetY = 2;
            }
            else if (isMouseOver)
            {
                state = ControlState.MouseOver;
                offsetX = -1;
                offsetY = -2;
            }
            else if (hasFocus)
            {
                state = ControlState.Focus;
            }

            // Background fill layer
            Element e = elementList[Button.ButtonLayer] as Element;
            float blendRate = (state == ControlState.Pressed) ? 0.0f : 0.8f;
            
            System.Drawing.Rectangle buttonRect = boundingBox;
            buttonRect.Offset(offsetX, offsetY);
            
            // Blend current color
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            e.FontColor.Blend(state, elapsedTime, blendRate);

            // Draw sprite/text of button
            parentDialog.DrawSprite(e, buttonRect);
            parentDialog.DrawText(textData, e, buttonRect);

            // Main button
            e = elementList[Button.FillLayer] as Element;
            
            // Blend current color
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            e.FontColor.Blend(state, elapsedTime, blendRate);

            parentDialog.DrawSprite(e, buttonRect);
            parentDialog.DrawText(textData, e, buttonRect);
        }

    }
    #endregion

    #region Checkbox Control
    /// <summary>
    /// Checkbox control
    /// </summary>
    public class Checkbox : Button
    {
        public const int BoxLayer = 0;
        public const int CheckLayer = 1;
        #region Event code
        public event EventHandler Changed;
        /// <summary>Create new button instance</summary>
        protected void RaiseChangedEvent(Checkbox sender, bool wasTriggeredByUser)
        {
            // Discard events triggered programatically if these types of events haven't been
            // enabled
            if (!Parent.IsUsingNonUserEvents && !wasTriggeredByUser)
                return;

            // Fire both the changed and clicked event
            base.RaiseClickEvent(sender, wasTriggeredByUser);
            if (Changed != null)
                Changed(sender, EventArgs.Empty);
        }
        #endregion
        protected System.Drawing.Rectangle buttonRect;
        protected System.Drawing.Rectangle textRect;
        protected bool isBoxChecked;

        /// <summary>
        /// Create new checkbox instance
        /// </summary>
        public Checkbox(Dialog parent) : base(parent)
        {
            controlType = ControlType.CheckBox;
            isBoxChecked = false;
            parentDialog = parent;
        }

        /// <summary>
        /// Checked property
        /// </summary>
        public virtual bool IsChecked
        {
            get { return isBoxChecked; }
            set { SetCheckedInternal(value, false); }
        }
        /// <summary>
        /// Sets the checked state and fires the event if necessary
        /// </summary>
        protected virtual void SetCheckedInternal(bool ischecked, bool fromInput)
        {
            isBoxChecked = ischecked;
            RaiseChangedEvent(this, fromInput);
        }

        /// <summary>
        /// Override hotkey to fire event
        /// </summary>
        public override void OnHotKey()
        {
            SetCheckedInternal(!isBoxChecked, true);
        }

        /// <summary>
        /// Does the control contain the point?
        /// </summary>
        public override bool ContainsPoint(System.Drawing.Point pt)
        {
            return (boundingBox.Contains(pt) || buttonRect.Contains(pt));
        }
        /// <summary>
        /// Update the rectangles
        /// </summary>
        protected override void UpdateRectangles()
        {
            // Update base first
            base.UpdateRectangles();

            // Update the two rects
            buttonRect = boundingBox;
            buttonRect = new System.Drawing.Rectangle(boundingBox.Location,
                new System.Drawing.Size(boundingBox.Height, boundingBox.Height));

            textRect = boundingBox;
            textRect.Offset((int) (1.25f * buttonRect.Width), 0);
        }

        /// <summary>
        /// Render the checkbox control
        /// </summary>
        public override void Render(Device device, float elapsedTime)
        {
            ControlState state = ControlState.Normal;
            if (IsVisible == false)
                state = ControlState.Hidden;
            else if (IsEnabled == false)
                state = ControlState.Disabled;
            else if (isPressed)
                state = ControlState.Pressed;
            else if (isMouseOver)
                state = ControlState.MouseOver;
            else if (hasFocus)
                state = ControlState.Focus;
 
            Element e = elementList[Checkbox.BoxLayer] as Element;
            float blendRate = (state == ControlState.Pressed) ? 0.0f : 0.8f;
            
            // Blend current color
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            e.FontColor.Blend(state, elapsedTime, blendRate);

            // Draw sprite/text of checkbox
            parentDialog.DrawSprite(e, buttonRect);
            parentDialog.DrawText(textData, e, textRect);

            if (!isBoxChecked)
                state = ControlState.Hidden;

            e = elementList[Checkbox.CheckLayer] as Element;
            // Blend current color
            e.TextureColor.Blend(state, elapsedTime, blendRate);

            // Draw sprite of checkbox
            parentDialog.DrawSprite(e, buttonRect);
        }

        /// <summary>
        /// Handle the keyboard for the checkbox
        /// </summary>
        public override bool HandleKeyboard(NativeMethods.WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false;

            switch(msg)
            {
                case NativeMethods.WindowMessage.KeyDown:
                    if ((System.Windows.Forms.Keys)wParam.ToInt32() == System.Windows.Forms.Keys.Space)
                    {
                        isPressed = true;
                        return true;
                    }
                    break;
                case NativeMethods.WindowMessage.KeyUp:
                    if ((System.Windows.Forms.Keys)wParam.ToInt32() == System.Windows.Forms.Keys.Space)
                    {
                        if (isPressed)
                        {
                            isPressed = false;
                            SetCheckedInternal(!isBoxChecked, true);
                        }
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Handle mouse messages from the checkbox
        /// </summary>
        public override bool HandleMouse(NativeMethods.WindowMessage msg, System.Drawing.Point pt, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false;

            switch(msg)
            {
                case NativeMethods.WindowMessage.LeftButtonDoubleClick:
                case NativeMethods.WindowMessage.LeftButtonDown:
                {
                    if (ContainsPoint(pt))
                    {
                        // Pressed while inside the control
                        isPressed = true;
                        Parent.SampleFramework.Window.Capture = true;
                        if ( (!hasFocus) && (parentDialog.IsUsingKeyboardInput) )
                            Dialog.RequestFocus(this);

                        return true;
                    }
                }
                    break;
                case NativeMethods.WindowMessage.LeftButtonUp:
                {
                    if (isPressed)
                    {
                        isPressed = false;
                        Parent.SampleFramework.Window.Capture = false;

                        // Button click
                        if (ContainsPoint(pt))
                        {
                            SetCheckedInternal(!isBoxChecked, true);
                        }
                        
                        return true;
                    }
                }
                    break;
            }

            return false;
        }
    }
    #endregion

    #region RadioButton Control
    /// <summary>
    /// Radio button control
    /// </summary>
    public class RadioButton : Checkbox
    {
        protected uint buttonGroupIndex;
        /// <summary>
        /// Create new radio button instance
        /// </summary>
        public RadioButton(Dialog parent) : base(parent)
        {
            controlType = ControlType.RadioButton;
            parentDialog = parent;
        }

        /// <summary>
        /// Button Group property
        /// </summary>
        public uint ButtonGroup
        {
            get { return buttonGroupIndex; }
            set { buttonGroupIndex = value; }
        }

        /// <summary>
        /// Sets the check state and potentially clears the group
        /// </summary>
        public void SetChecked(bool ischecked, bool clear)
        {
            SetCheckedInternal(ischecked, clear, false); 
        }

        /// <summary>
        /// Sets the checked state and fires the event if necessary
        /// </summary>
        protected virtual void SetCheckedInternal(bool ischecked, bool clearGroup, bool fromInput)
        {
            isBoxChecked = ischecked;
            RaiseChangedEvent(this, fromInput);
        }

        /// <summary>
        /// Override hotkey to fire event
        /// </summary>
        public override void OnHotKey()
        {
            SetCheckedInternal(true, true);
        }

        /// <summary>
        /// Handle the keyboard for the checkbox
        /// </summary>
        public override bool HandleKeyboard(NativeMethods.WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false;

            switch(msg)
            {
                case NativeMethods.WindowMessage.KeyDown:
                    if ((System.Windows.Forms.Keys)wParam.ToInt32() == System.Windows.Forms.Keys.Space)
                    {
                        isPressed = true;
                        return true;
                    }
                    break;
                case NativeMethods.WindowMessage.KeyUp:
                    if ((System.Windows.Forms.Keys)wParam.ToInt32() == System.Windows.Forms.Keys.Space)
                    {
                        if (isPressed)
                        {
                            isPressed = false;
                            parentDialog.ClearRadioButtonGroup(buttonGroupIndex);
                            isBoxChecked = !isBoxChecked;

                            RaiseChangedEvent(this, true);
                        }
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Handle mouse messages from the radio button
        /// </summary>
        public override bool HandleMouse(NativeMethods.WindowMessage msg, System.Drawing.Point pt, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false;

            switch(msg)
            {
                case NativeMethods.WindowMessage.LeftButtonDoubleClick:
                case NativeMethods.WindowMessage.LeftButtonDown:
                {
                    if (ContainsPoint(pt))
                    {
                        // Pressed while inside the control
                        isPressed = true;
                        Parent.SampleFramework.Window.Capture = true;
                        if ( (!hasFocus) && (parentDialog.IsUsingKeyboardInput) )
                            Dialog.RequestFocus(this);

                        return true;
                    }
                }
                    break;
                case NativeMethods.WindowMessage.LeftButtonUp:
                {
                    if (isPressed)
                    {
                        isPressed = false;
                        Parent.SampleFramework.Window.Capture = false;

                        // Button click
                        if (ContainsPoint(pt))
                        {
                            parentDialog.ClearRadioButtonGroup(buttonGroupIndex);
                            isBoxChecked = !isBoxChecked;

                            RaiseChangedEvent(this, true);
                        }
                        
                        return true;
                    }
                }
                    break;
            }

            return false;
        }
    }
    #endregion

    #region Scrollbar control
    /// <summary>
    /// A scroll bar control
    /// </summary>
    public class ScrollBar : Control
    {
        public const int TrackLayer = 0;
        public const int UpButtonLayer = 1;
        public const int DownButtonLayer = 2;
        public const int ThumbLayer = 3;
        protected const int MinimumThumbSize = 8;
        #region Instance Data
        protected bool showingThumb;
        protected System.Drawing.Rectangle upButtonRect;
        protected System.Drawing.Rectangle downButtonRect;
        protected System.Drawing.Rectangle trackRect;
        protected System.Drawing.Rectangle thumbRect;
        protected int position; // Position of the first displayed item
        protected int pageSize; // How many items are displayable in one page
        protected int start; // First item
        protected int end; // The index after the last item
        private int thumbOffsetY;
        private bool isDragging;
        #endregion

        /// <summary>
        /// Creates a new instance of the scroll bar class
        /// </summary>
        public ScrollBar(Dialog parent) : base(parent)
        {
            // Store parent and control type
            controlType = ControlType.Scrollbar;
            parentDialog = parent;

            // Set default properties
            showingThumb = true;
            upButtonRect = System.Drawing.Rectangle.Empty;
            downButtonRect = System.Drawing.Rectangle.Empty;
            trackRect = System.Drawing.Rectangle.Empty;
            thumbRect = System.Drawing.Rectangle.Empty;

            position = 0;
            pageSize = 1;
            start = 0;
            end = 1;
        }

        /// <summary>
        /// Update all of the rectangles
        /// </summary>
        protected override void UpdateRectangles()
        {
            // Get the bounding box first
            base.UpdateRectangles();

            // Make sure buttons are square
            upButtonRect = new System.Drawing.Rectangle(boundingBox.Location,
                new System.Drawing.Size(boundingBox.Width, boundingBox.Width));

            downButtonRect = new System.Drawing.Rectangle(boundingBox.Left, boundingBox.Bottom - boundingBox.Width,
                boundingBox.Width, boundingBox.Width);

            trackRect = new System.Drawing.Rectangle(upButtonRect.Left, upButtonRect.Bottom, 
                upButtonRect.Width, downButtonRect.Top - upButtonRect.Bottom);

            thumbRect = upButtonRect;

            UpdateThumbRectangle();
        }

        /// <summary>
        /// Position of the track
        /// </summary>
        public int TrackPosition
        {
            get { return position; }
            set { position = value; Cap(); UpdateThumbRectangle(); }
        }
        /// <summary>
        /// Size of a 'page'
        /// </summary>
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = value; Cap(); UpdateThumbRectangle(); }
        }

        /// <summary>Clips position at boundaries</summary>
        protected void Cap()
        {
            if (position < start || end - start <= pageSize)
            {
                position = start;
            }
            else if (position + pageSize > end)
                position = end - pageSize;
        }

        /// <summary>Compute the dimension of the scroll thumb</summary>
        protected void UpdateThumbRectangle()
        {
            if (end - start > pageSize)
            {
                int thumbHeight = Math.Max(trackRect.Height * pageSize / (end-start), MinimumThumbSize);
                int maxPosition = end - start - pageSize;
                thumbRect.Location = new System.Drawing.Point(thumbRect.Left,
                    trackRect.Top + (position - start) * (trackRect.Height - thumbHeight) / maxPosition);
                thumbRect.Size = new System.Drawing.Size(thumbRect.Width, thumbHeight);
                showingThumb = true;
            }
            else
            {
                // No content to scroll
                thumbRect.Height = 0;
                showingThumb = false;
            }
        }

        /// <summary>Scrolls by delta items.  A positive value scrolls down, while a negative scrolls down</summary>
        public void Scroll(int delta)
        {
            // Perform scroll
            position += delta;
            // Cap position
            Cap();
            // Update thumb rectangle
            UpdateThumbRectangle();
        }

        /// <summary>Shows an item</summary>
        public void ShowItem(int index)
        {
            // Cap the index
            if (index < 0)
                index = 0;

            if (index >= end)
                index = end - 1;

            // Adjust the position to show this item
            if (position > index)
                position = index;
            else if (position + pageSize <= index)
                position = index - pageSize + 1;

            // Update thumbs again
            UpdateThumbRectangle();
        }

        /// <summary>Sets the track range</summary>
        public void SetTrackRange(int startRange, int endRange)
        {
            start = startRange; end = endRange;
            Cap();
            UpdateThumbRectangle();
        }

        /// <summary>Render the scroll bar control</summary>
        public override void Render(Device device, float elapsedTime)
        {
            ControlState state = ControlState.Normal;
            if (IsVisible == false)
                state = ControlState.Hidden;
            else if ( (IsEnabled == false) || (showingThumb == false) )
                state = ControlState.Disabled;
            else if (isMouseOver)
                state = ControlState.MouseOver;
            else if (hasFocus)
                state = ControlState.Focus;

            float blendRate = (state == ControlState.Pressed) ? 0.0f : 0.8f;

            // Background track layer
            Element e = elementList[ScrollBar.TrackLayer] as Element;
            
            // Blend current color
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            parentDialog.DrawSprite(e, trackRect);

            // Up arrow
            e = elementList[ScrollBar.UpButtonLayer] as Element;
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            parentDialog.DrawSprite(e, upButtonRect);

            // Down arrow
            e = elementList[ScrollBar.DownButtonLayer] as Element;
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            parentDialog.DrawSprite(e, downButtonRect);

            // Thumb button
            e = elementList[ScrollBar.ThumbLayer] as Element;
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            parentDialog.DrawSprite(e, thumbRect);
        }

        /// <summary>Stores data for a combo box item</summary>
        public override bool HandleMouse(NativeMethods.WindowMessage msg, System.Drawing.Point pt, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false;

            switch(msg)
            {
                case NativeMethods.WindowMessage.LeftButtonDoubleClick:
                case NativeMethods.WindowMessage.LeftButtonDown:
                {
                    Parent.SampleFramework.Window.Capture = true;

                    // Check for on up button
                    if (upButtonRect.Contains(pt))
                    {
                        if (position > start)
                            --position;
                        UpdateThumbRectangle();
                        return true;
                    }

                    // Check for on down button
                    if (downButtonRect.Contains(pt))
                    {
                        if (position + pageSize < end)
                            ++position;
                        UpdateThumbRectangle();
                        return true;
                    }

                    // Check for click on thumb
                    if (thumbRect.Contains(pt))
                    {
                        isDragging = true;
                        thumbOffsetY = pt.Y - thumbRect.Top;
                        return true;
                    }

                    // check for click on track
                    if (thumbRect.Left <= pt.X &&
                        thumbRect.Right > pt.X)
                    {
                        if (thumbRect.Top > pt.Y &&
                            trackRect.Top <= pt.Y)
                        {
                            Scroll(-(pageSize-1));
                            return true;
                        }
                        else if (thumbRect.Bottom <= pt.Y &&
                            trackRect.Bottom > pt.Y)
                        {
                            Scroll(pageSize-1);
                            return true;
                        }
                    }

                    break;
                }
                case NativeMethods.WindowMessage.LeftButtonUp:
                {
                    isDragging = false;
                    Parent.SampleFramework.Window.Capture = false;
                    UpdateThumbRectangle();
                    break;
                }

                case NativeMethods.WindowMessage.MouseMove:
                {
                    if (isDragging)
                    {
                        // Calculate new bottom and top of thumb rect
                        int bottom = thumbRect.Bottom + (pt.Y - thumbOffsetY - thumbRect.Top);
                        int top = pt.Y - thumbOffsetY;
                        thumbRect = new System.Drawing.Rectangle(thumbRect.Left, top, thumbRect.Width, bottom - top);
                        if (thumbRect.Top < trackRect.Top)
                            thumbRect.Offset(0, trackRect.Top - thumbRect.Top);
                        else if (thumbRect.Bottom > trackRect.Bottom)
                            thumbRect.Offset(0, trackRect.Bottom - thumbRect.Bottom);

                        // Compute first item index based on thumb position
                        int maxFirstItem = end - start - pageSize; // Largest possible index for first item
                        int maxThumb = trackRect.Height - thumbRect.Height; // Largest possible thumb position

                        position = start + (thumbRect.Top - trackRect.Top +
                            maxThumb / (maxFirstItem * 2) ) * // Shift by half a row to avoid last row covered
                            maxFirstItem / maxThumb;

                        return true;
                    }
                    break;
                }
            }

            // Was not handled
            return false;
        }

    }
    #endregion

    #region ComboBox Control
    /// <summary>Stores data for a combo box item</summary>
    public struct ComboBoxItem
    {
        public string ItemText;
        public object ItemData;
        public System.Drawing.Rectangle ItemRect;
        public bool IsItemVisible;
    }

    /// <summary>Combo box control</summary>
    public class ComboBox : Button
    {
        public const int MainLayer = 0;
        public const int ComboButtonLayer = 1;
        public const int DropdownLayer = 2;
        public const int SelectionLayer = 3;
        #region Event code
        public event EventHandler Changed;
        /// <summary>Create new button instance</summary>
        protected void RaiseChangedEvent(ComboBox sender, bool wasTriggeredByUser)
        {
            // Discard events triggered programatically if these types of events haven't been
            // enabled
            if (!Parent.IsUsingNonUserEvents && !wasTriggeredByUser)
                return;

            // Fire both the changed and clicked event
            base.RaiseClickEvent(sender, wasTriggeredByUser);
            if (Changed != null)
                Changed(sender, EventArgs.Empty);
        }
        #endregion
        private bool isScrollBarInit;

        #region Instance data
        protected int selectedIndex;
        protected int focusedIndex;
        protected int dropHeight;
        protected ScrollBar scrollbarControl;
        protected int scrollWidth;
        protected bool isComboOpen;
        protected System.Drawing.Rectangle textRect;
        protected System.Drawing.Rectangle buttonRect;
        protected System.Drawing.Rectangle dropDownRect;
        protected System.Drawing.Rectangle dropDownTextRect;
        protected ArrayList itemList;
        #endregion

        /// <summary>Create new combo box control</summary>
        public ComboBox(Dialog parent) : base(parent)
        {
            // Store control type and parent dialog
            controlType = ControlType.ComboBox;
            parentDialog = parent;
            // Create the scrollbar control too
            scrollbarControl = new ScrollBar(parent);

            // Set some default items
            dropHeight = 100;
            scrollWidth = 16;
            selectedIndex = -1;
            focusedIndex = -1;
            isScrollBarInit = false;

            // Create the item list array
            itemList = new ArrayList();
        }

        /// <summary>Update the rectangles for the combo box control</summary>
        protected override void UpdateRectangles()
        {
            // Get bounding box
            base.UpdateRectangles();

            // Update the bounding box for the items
            buttonRect = new System.Drawing.Rectangle(boundingBox.Right - boundingBox.Height, boundingBox.Top,
                boundingBox.Height, boundingBox.Height);

            textRect = boundingBox;
            textRect.Size = new System.Drawing.Size(textRect.Width - buttonRect.Width, textRect.Height);

            dropDownRect = textRect;
            dropDownRect.Offset(0, (int)(0.9f * textRect.Height));
            dropDownRect.Size = new System.Drawing.Size(dropDownRect.Width - scrollWidth, dropDownRect.Height + dropHeight);

            // Scale it down slightly
            System.Drawing.Point loc = dropDownRect.Location;
            System.Drawing.Size size = dropDownRect.Size;

            loc.X += (int)(0.1f * dropDownRect.Width);
            loc.Y += (int)(0.1f * dropDownRect.Height);
            size.Width -= (2 * (int)(0.1f * dropDownRect.Width));
            size.Height -= (2 * (int)(0.1f * dropDownRect.Height));

            dropDownTextRect = new System.Drawing.Rectangle(loc, size);

            // Update the scroll bars rects too
            scrollbarControl.SetLocation(dropDownRect.Right, dropDownRect.Top + 2);
            scrollbarControl.SetSize(scrollWidth, dropDownRect.Height - 2);
            FontNode fNode = DialogResourceManager.GetGlobalInstance().GetFontNode((int)(elementList[2] as Element).FontIndex);
            if ((fNode != null) && (fNode.Height > 0))
            {
                scrollbarControl.PageSize = (int)(dropDownTextRect.Height / fNode.Height);

                // The selected item may have been scrolled off the page.
                // Ensure that it is in page again.
                scrollbarControl.ShowItem(selectedIndex);
            }
        }

        /// <summary>Sets the drop height of this control</summary>
        public void SetDropHeight(int height) { dropHeight = height; UpdateRectangles(); }
        /// <summary>Sets the scroll bar width of this control</summary>
        public void SetScrollbarWidth(int width) { scrollWidth = width; UpdateRectangles(); }
        /// <summary>Can this control have focus</summary>
        public override bool CanHaveFocus { get { return (IsVisible && IsEnabled); } }
        /// <summary>Number of items current in the list</summary>
        public int NumberItems { get { return itemList.Count; } }
        /// <summary>Indexer for items in the list</summary>
        public ComboBoxItem this[int index]
        {
            get { return (ComboBoxItem)itemList[index]; }
        }

        /// <summary>Initialize the scrollbar control here</summary>
        public override void OnInitialize()
        {
            parentDialog.InitializeControl(scrollbarControl);
        }

        /// <summary>Called when focus leaves the control</summary>
        public override void OnFocusOut()
        {
            // Call base first
            base.OnFocusOut ();
            isComboOpen = false;
        }
        /// <summary>Called when the control's hotkey is pressed</summary>
        public override void OnHotKey()
        {
            if (isComboOpen)
                return; // Nothing to do yet

            if (selectedIndex == -1)
                return; // Nothing selected

            selectedIndex++;
            if (selectedIndex >= itemList.Count)
                selectedIndex = 0;

            focusedIndex = selectedIndex;
            RaiseChangedEvent(this, true);
        }


        /// <summary>Called when the control needs to handle the keyboard</summary>
        public override bool HandleKeyboard(NativeMethods.WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            const uint RepeatMask = (0x40000000);

            if (!IsEnabled || !IsVisible)
                return false;

            // Let the scroll bar have a chance to handle it first
            if (scrollbarControl.HandleKeyboard(msg, wParam, lParam))
                return true;

            switch (msg)
            {
                case NativeMethods.WindowMessage.KeyDown:
                {
                    switch((System.Windows.Forms.Keys)wParam.ToInt32())
                    {
                        case System.Windows.Forms.Keys.Return:
                        {
                            if (isComboOpen)
                            {
                                if (selectedIndex != focusedIndex)
                                {
                                    selectedIndex = focusedIndex;
                                    RaiseChangedEvent(this, true);
                                }
                                isComboOpen = false;

                                if (!Parent.IsUsingKeyboardInput)
                                    Dialog.ClearFocus();

                                return true;
                            }
                            break;
                        }
                        case System.Windows.Forms.Keys.F4:
                        {
                            // Filter out auto repeats
                            if ((lParam.ToInt32() & RepeatMask) != 0)
                                return true;

                            isComboOpen = !isComboOpen;
                            if (!isComboOpen)
                            {
                                RaiseChangedEvent(this, true);

                                if (!Parent.IsUsingKeyboardInput)
                                    Dialog.ClearFocus();
                            }

                            return true;
                        }
                        case System.Windows.Forms.Keys.Left:
                        case System.Windows.Forms.Keys.Up:
                        {
                            if (focusedIndex > 0)
                            {
                                focusedIndex--;
                                selectedIndex = focusedIndex;
                                if (!isComboOpen)
                                    RaiseChangedEvent(this, true);
                            }
                            return true;
                        }
                        case System.Windows.Forms.Keys.Right:
                        case System.Windows.Forms.Keys.Down:
                        {
                            if (focusedIndex + 1 < (int)NumberItems)
                            {
                                focusedIndex++;
                                selectedIndex = focusedIndex;
                                if (!isComboOpen)
                                    RaiseChangedEvent(this, true);
                            }
                            return true;
                        }
                    }
                    break;
                }
            }

            return false;
        }

        /// <summary>Called when the control should handle the mouse</summary>
        public override bool HandleMouse(NativeMethods.WindowMessage msg, System.Drawing.Point pt, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false; // Nothing to do

            // Let the scroll bar handle it first
            if (scrollbarControl.HandleMouse(msg, pt, wParam, lParam))
                return true;

            // Ok, scrollbar didn't handle it, move on
            switch(msg)
            {
                case NativeMethods.WindowMessage.MouseMove:
                {
                    if (isComboOpen && dropDownRect.Contains(pt))
                    {
                        // Determine which item has been selected
                        for (int i = 0; i < itemList.Count; i++)
                        {
                            ComboBoxItem cbi = (ComboBoxItem)itemList[i];
                            if (cbi.IsItemVisible && cbi.ItemRect.Contains(pt))
                            {
                                focusedIndex = i;
                            }
                        }
                        return true;
                    }
                    break;
                }
                case NativeMethods.WindowMessage.LeftButtonDoubleClick:
                case NativeMethods.WindowMessage.LeftButtonDown:
                {
                    if (ContainsPoint(pt))
                    {
                        // Pressed while inside the control
                        isPressed = true;
                        Parent.SampleFramework.Window.Capture = true;

                        if (!hasFocus)
                            Dialog.RequestFocus(this);

                        // Toggle dropdown
                        if (hasFocus)
                        {
                            isComboOpen = !isComboOpen;
                            if (!isComboOpen)
                            {
                                if (!parentDialog.IsUsingKeyboardInput)
                                    Dialog.ClearFocus();
                            }
                        }

                        return true;
                    }

                    // Perhaps this click is within the dropdown
                    if (isComboOpen && dropDownRect.Contains(pt))
                    {
                        // Determine which item has been selected
                        for (int i = scrollbarControl.TrackPosition; i < itemList.Count; i++)
                        {
                            ComboBoxItem cbi = (ComboBoxItem)itemList[i];
                            if (cbi.IsItemVisible && cbi.ItemRect.Contains(pt))
                            {
                                selectedIndex = focusedIndex = i;
                                RaiseChangedEvent(this, true);

                                isComboOpen = false;

                                if (!parentDialog.IsUsingKeyboardInput)
                                    Dialog.ClearFocus();

                                break;
                            }
                        }
                        return true;
                    }
                    // Mouse click not on main control or in dropdown, fire an event if needed
                    if (isComboOpen)
                    {
                        focusedIndex = selectedIndex;
                        RaiseChangedEvent(this, true);
                        isComboOpen = false;
                    }

                    // Make sure the control is no longer 'pressed'
                    isPressed = false;

                    // Release focus if appropriate
                    if (!parentDialog.IsUsingKeyboardInput)
                        Dialog.ClearFocus();

                    break;
                }
                case NativeMethods.WindowMessage.LeftButtonUp:
                {
                    if (isPressed && ContainsPoint(pt))
                    {
                        // Button click
                        isPressed = false;
                        Parent.SampleFramework.Window.Capture = false;
                        return true;
                    }
                    break;
                }
                case NativeMethods.WindowMessage.MouseWheel:
                {
                    int zdelta = (short)NativeMethods.HiWord((uint)wParam.ToInt32()) / Dialog.WheelDelta;
                    if (isComboOpen)
                    {
                        scrollbarControl.Scroll(-zdelta * System.Windows.Forms.SystemInformation.MouseWheelScrollLines);
                    }
                    else
                    {
                        if (zdelta > 0)
                        {
                            if (focusedIndex > 0)
                            {
                                focusedIndex--;
                                selectedIndex = focusedIndex;
                                if (!isComboOpen)
                                {
                                    RaiseChangedEvent(this, true);
                                }
                            }
                        }
                        else
                        {
                            if (focusedIndex +1 < NumberItems)
                            {
                                focusedIndex++;
                                selectedIndex = focusedIndex;
                                if (!isComboOpen)
                                {
                                    RaiseChangedEvent(this, true);
                                }
                            }
                        }
                    }
                    return true;
                }
            }

            // Didn't handle it
            return false;
        }

        /// <summary>Called when the control should be rendered</summary>
        public override void Render(Device device, float elapsedTime)
        {
            ControlState state = ControlState.Normal;
            if (!isComboOpen)
                state = ControlState.Hidden;

            // Dropdown box
            Element e = elementList[ComboBox.DropdownLayer] as Element;
            
            // If we have not initialized the scroll bar page size,
            // do that now.
            if (!isScrollBarInit)
            {
                FontNode fNode = DialogResourceManager.GetGlobalInstance().GetFontNode((int)e.FontIndex);
                if ((fNode != null) && (fNode.Height > 0))
                    scrollbarControl.PageSize = (int)(dropDownTextRect.Height / fNode.Height);
                else
                    scrollbarControl.PageSize = dropDownTextRect.Height;

                isScrollBarInit = true;
            }

            if (isComboOpen)
                scrollbarControl.Render(device, elapsedTime);

            // Blend current color
            e.TextureColor.Blend(state, elapsedTime);
            e.FontColor.Blend(state, elapsedTime);
            parentDialog.DrawSprite(e, dropDownRect);

            // Selection outline
            Element selectionElement = elementList[ComboBox.SelectionLayer] as Element;
            selectionElement.TextureColor.Current = e.TextureColor.Current;
            selectionElement.FontColor.Current = selectionElement.FontColor.States[(int)ControlState.Normal];

            FontNode font = DialogResourceManager.GetGlobalInstance().GetFontNode((int)e.FontIndex);
            int currentY = dropDownTextRect.Top;
            int remainingHeight = dropDownTextRect.Height;

            for (int i = scrollbarControl.TrackPosition; i < itemList.Count; i++)
            {
                ComboBoxItem cbi = (ComboBoxItem)itemList[i];

                // Make sure there's room left in the dropdown
                remainingHeight -= (int)font.Height;
                if (remainingHeight < 0)
                {
                    // Not visible, store that item
                    cbi.IsItemVisible = false;
                    itemList[i] = cbi; // Store this back in list
                    continue;
                }

                cbi.ItemRect = new System.Drawing.Rectangle(dropDownTextRect.Left, currentY,
                    dropDownTextRect.Width, (int)font.Height);
                cbi.IsItemVisible = true;
                currentY += (int)font.Height;
                itemList[i] = cbi; // Store this back in list

                if (isComboOpen)
                {
                    if (focusedIndex == i)
                    {
                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(
                            dropDownRect.Left, cbi.ItemRect.Top - 2, dropDownRect.Width,
                            cbi.ItemRect.Height + 4);
                        parentDialog.DrawSprite(selectionElement, rect);
                        parentDialog.DrawText(cbi.ItemText, selectionElement, cbi.ItemRect);
                    }
                    else
                    {
                        parentDialog.DrawText(cbi.ItemText, e, cbi.ItemRect);
                    }
                }
            }

            int offsetX = 0;
            int offsetY = 0;

            state = ControlState.Normal;
            if (IsVisible == false)
                state = ControlState.Hidden;
            else if (IsEnabled == false)
                state = ControlState.Disabled;
            else if (isPressed)
            {
                state = ControlState.Pressed;
                offsetX = 1;
                offsetY = 2;
            }
            else if (isMouseOver)
            {
                state = ControlState.MouseOver;
                offsetX = -1;
                offsetY = -2;
            }
            else if (hasFocus)
                state = ControlState.Focus;

            float blendRate = (state == ControlState.Pressed) ? 0.0f : 0.8f;

            // Button
            e = elementList[ComboBox.ComboButtonLayer] as Element;
            
            // Blend current color
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            
            System.Drawing.Rectangle windowRect = buttonRect;
            windowRect.Offset(offsetX, offsetY);
            // Draw sprite
            parentDialog.DrawSprite(e, windowRect);

            if (isComboOpen)
                state = ControlState.Pressed;

            // Main text box
            e = elementList[ComboBox.MainLayer] as Element;
            
            // Blend current color
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            e.FontColor.Blend(state, elapsedTime, blendRate);

            // Draw sprite
            parentDialog.DrawSprite(e, textRect);

            if (selectedIndex >= 0 && selectedIndex < itemList.Count)
            {
                try
                {
                    ComboBoxItem cbi = (ComboBoxItem)itemList[selectedIndex];
                    parentDialog.DrawText(cbi.ItemText, e, textRect);
                }
                catch {} // Ignore
            }

        }

        #region Item Controlling methods
        /// <summary>Adds an item to the combo box control</summary>
        public void AddItem(string text, object data)
        {
            if ((text == null) || (text.Length == 0))
                throw new ArgumentNullException("text", "You must pass in a valid item name when adding a new item.");

            // Create a new item and add it
            ComboBoxItem newitem = new ComboBoxItem();
            newitem.ItemText = text;
            newitem.ItemData = data;
            itemList.Add(newitem);

            // Update the scrollbar with the new range
            scrollbarControl.SetTrackRange(0, itemList.Count);

            // If this is the only item in the list, it should be selected
            if (NumberItems == 1)
            {
                selectedIndex = 0;
                focusedIndex = 0;
                RaiseChangedEvent(this, false);
            }
        }

        /// <summary>Removes an item at a particular index</summary>
        public void RemoveAt(int index)
        {
            // Remove the item
            itemList.RemoveAt(index);

            // Update the scrollbar with the new range
            scrollbarControl.SetTrackRange(0, itemList.Count);

            if (selectedIndex >= itemList.Count)
                selectedIndex = itemList.Count - 1;
        }

        /// <summary>Removes all items from the control</summary>
        public void Clear()
        {
            // clear the list
            itemList.Clear();

            // Update scroll bar and index
            scrollbarControl.SetTrackRange(0, 1);
            focusedIndex = selectedIndex = -1;
        }

        /// <summary>Determines whether this control contains an item</summary>
        public bool ContainsItem(string text, int start)
        {
            return (FindItem(text, start) != -1);
        }
        /// <summary>Determines whether this control contains an item</summary>
        public bool ContainsItem(string text) { return ContainsItem(text, 0); }

        /// <summary>Gets the data for the selected item</summary>
        public object GetSelectedData()
        {
            if (selectedIndex < 0)
                return null; // Nothing selected

            ComboBoxItem cbi = (ComboBoxItem)itemList[selectedIndex];
            return cbi.ItemData;
        }

        /// <summary>Gets the selected item</summary>
        public ComboBoxItem GetSelectedItem()
        {
            if (selectedIndex < 0)
                throw new ArgumentOutOfRangeException("selectedIndex", "No item selected.");

            return (ComboBoxItem)itemList[selectedIndex];
        }

        /// <summary>Gets the data for an item</summary>
        public object GetItemData(string text)
        {
            int index = FindItem(text);
            if (index == -1)
                return null; // no item

            ComboBoxItem cbi = (ComboBoxItem)itemList[index];
            return cbi.ItemData;
        }

        /// <summary>Finds an item in the list and returns the index</summary>
        protected int FindItem(string text, int start)
        {
            if ((text == null) || (text.Length == 0))
                return -1;

            for(int i = start; i < itemList.Count; i++)
            {
                ComboBoxItem cbi = (ComboBoxItem)itemList[i];
                if (string.Compare(cbi.ItemText, text, true) == 0)
                {
                    return i;
                }
            }

            // Never found it
            return -1;
        }
        /// <summary>Finds an item in the list and returns the index</summary>
        protected int FindItem(string text) { return FindItem(text, 0); }

        /// <summary>Sets the selected item by index</summary>
        public void SetSelected(int index)
        {
            if (index >= NumberItems)
                throw new ArgumentOutOfRangeException("index", "There are not enough items in the list to select this index.");

            focusedIndex = selectedIndex = index;
            RaiseChangedEvent(this, false);
        }

        /// <summary>Sets the selected item by text</summary>
        public void SetSelected(string text)
        {
            if ((text == null) || (text.Length == 0))
                throw new ArgumentNullException("text", "You must pass in a valid item name when adding a new item.");

            int index = FindItem(text);
            if (index == -1)
                throw new InvalidOperationException("This item could not be found.");

            focusedIndex = selectedIndex = index;
            RaiseChangedEvent(this, false);
        }

        /// <summary>Sets the selected item by data</summary>
        public void SetSelectedByData(object data)
        {
            for (int index = 0; index < itemList.Count; index++)
            {
                ComboBoxItem cbi = (ComboBoxItem)itemList[index];
                if (cbi.ItemData.ToString().Equals(data.ToString()))
                {
                    focusedIndex = selectedIndex = index;
                    RaiseChangedEvent(this, false);
                }
            }

            // Could not find this item.  Uncomment line below for debug information
            //System.Diagnostics.Debugger.Log(9,string.Empty, "Could not find an object with this data.\r\n");
        }

        #endregion
    }
    #endregion

    #region Slider Control
    /// <summary>Slider control</summary>
    public class Slider : Control
    {
        public const int TrackLayer = 0;
        public const int ButtonLayer = 1;
        #region Instance Data
        public event EventHandler ValueChanged;
        protected int currentValue; 
        protected int maxValue;
        protected int minValue;
        
        protected int dragX; // Mouse position at the start of the drag
        protected int dragOffset; // Drag offset from the center of the button
        protected int buttonX;

        protected bool isPressed;
        protected System.Drawing.Rectangle buttonRect;

        /// <summary>Slider's can always have focus</summary>
        public override bool CanHaveFocus { get { return true; }}

        /// <summary>Current value of the slider</summary>
        protected void RaiseValueChanged(Slider sender, bool wasTriggeredByUser)
        {
            // Discard events triggered programatically if these types of events haven't been
            // enabled
            if (!Parent.IsUsingNonUserEvents && !wasTriggeredByUser)
                return;

            if (ValueChanged != null)
                ValueChanged(sender, EventArgs.Empty);
        }
        /// <summary>Current value of the slider</summary>
        public int Value { get { return currentValue; } set { SetValueInternal(value, false); } }
        /// <summary>Sets the range of the slider</summary>
        public void SetRange(int min, int max)
        {
            minValue = min;
            maxValue = max;
            SetValueInternal(currentValue, false);
        }

        /// <summary>Sets the value internally and fires the event if needed</summary>
        protected void SetValueInternal(int newValue, bool fromInput)
        {
            // Clamp to the range
            newValue = Math.Max(minValue, newValue);
            newValue = Math.Min(maxValue, newValue);
            if (newValue == currentValue)
                return;

            // Update the value, the rects, then fire the events if necessar
            currentValue = newValue; 
            UpdateRectangles();
            RaiseValueChanged(this, fromInput);
        }
        #endregion

        /// <summary>Create new button instance</summary>
        public Slider(Dialog parent): base(parent)
        {
            controlType = ControlType.Slider;
            parentDialog = parent;

            isPressed = false;
            minValue = 0;
            maxValue = 100;
            currentValue = 50;
        }

        /// <summary>Does the control contain this point?</summary>
        public override bool ContainsPoint(System.Drawing.Point pt)
        {
            return boundingBox.Contains(pt) || buttonRect.Contains(pt);
        }

        /// <summary>Update the rectangles for the control</summary>
        protected override void UpdateRectangles()
        {
            // First get the bounding box
            base.UpdateRectangles ();

            // Create the button rect
            buttonRect = boundingBox;
            buttonRect.Width = buttonRect.Height; // Make it square

            // Offset it 
            buttonRect.Offset(-buttonRect.Width / 2, 0);
            buttonX = (int)((currentValue - minValue) * (float)boundingBox.Width / (maxValue - minValue) );
            buttonRect.Offset(buttonX, 0);
        }

        /// <summary>Gets a value from a position</summary>
        public int ValueFromPosition(int x)
        {
            float valuePerPixel = ((float)(maxValue - minValue) / (float)boundingBox.Width);
            return (int)(0.5f + minValue + valuePerPixel * (x - boundingBox.Left));
        }

        /// <summary>Handle mouse input input</summary>
        public override bool HandleMouse(Microsoft.Samples.DirectX.UtilityToolkit.NativeMethods.WindowMessage msg, System.Drawing.Point pt, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false;

            switch(msg)
            {
                case NativeMethods.WindowMessage.LeftButtonDoubleClick:
                case NativeMethods.WindowMessage.LeftButtonDown:
                {
                    if (buttonRect.Contains(pt))
                    {
                        // Pressed while inside the control
                        isPressed = true;
                        Parent.SampleFramework.Window.Capture = true;

                        dragX = pt.X;
                        dragOffset = buttonX - dragX;
                        if (!hasFocus)
                            Dialog.RequestFocus(this);
                        
                        return true;
                    }
                    if (boundingBox.Contains(pt))
                    {
                        if (pt.X > buttonX + controlX)
                        {
                            SetValueInternal(currentValue + 1, true);
                            return true;
                        }
                        if (pt.X < buttonX + controlX)
                        {
                            SetValueInternal(currentValue - 1, true);
                            return true;
                        }
                    }

                    break;
                }
                case NativeMethods.WindowMessage.LeftButtonUp:
                {
                    if (isPressed)
                    {
                        isPressed = false;
                        Parent.SampleFramework.Window.Capture = false;
                        Dialog.ClearFocus();
                        RaiseValueChanged(this, true);
                        return true;
                    }
                    break;
                }
                case NativeMethods.WindowMessage.MouseMove:
                {
                    if (isPressed)
                    {
                        SetValueInternal(ValueFromPosition(controlX + pt.X + dragOffset), true);
                        return true;
                    }
                    break;
                }
            }
            return false;
        }

        /// <summary>Handle keyboard input</summary>
        public override bool HandleKeyboard(Microsoft.Samples.DirectX.UtilityToolkit.NativeMethods.WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false;

            if (msg == NativeMethods.WindowMessage.KeyDown)
            {
                switch((System.Windows.Forms.Keys)wParam.ToInt32())
                {
                    case System.Windows.Forms.Keys.Home:
                        SetValueInternal(minValue, true);
                        return true;
                    case System.Windows.Forms.Keys.End:
                        SetValueInternal(maxValue, true);
                        return true;
                    case System.Windows.Forms.Keys.Prior:
                    case System.Windows.Forms.Keys.Left:
                    case System.Windows.Forms.Keys.Up:
                        SetValueInternal(currentValue - 1, true);
                        return true;
                    case System.Windows.Forms.Keys.Next:
                    case System.Windows.Forms.Keys.Right:
                    case System.Windows.Forms.Keys.Down:
                        SetValueInternal(currentValue + 1, true);
                        return true;
                }
            }

            return false;
        }

    
        /// <summary>Render the slider</summary>
        public override void Render(Device device, float elapsedTime)
        {
            ControlState state = ControlState.Normal;
            if (IsVisible == false)
            {
                state = ControlState.Hidden;
            }
            else if (IsEnabled == false)
            {
                state = ControlState.Disabled;
            }
            else if (isPressed)
            {
                state = ControlState.Pressed;
            }
            else if (isMouseOver)
            {
                state = ControlState.MouseOver;
            }
            else if (hasFocus)
            {
                state = ControlState.Focus;
            }

            float blendRate = (state == ControlState.Pressed) ? 0.0f : 0.8f;

            Element e = elementList[Slider.TrackLayer] as Element;
            
            // Blend current color
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            parentDialog.DrawSprite(e, boundingBox);

            e = elementList[Slider.ButtonLayer] as Element;
            // Blend current color
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            parentDialog.DrawSprite(e, buttonRect);
        }
    }
    #endregion

    #region Listbox Control

    /// <summary>Style of the list box</summary>
    public enum ListBoxStyle
    {
        SingleSelection,
        Multiselection,
    }

    /// <summary>Stores data for a list box item</summary>
    public struct ListBoxItem
    {
        public string ItemText;
        public object ItemData;
        public System.Drawing.Rectangle ItemRect;
        public bool IsItemSelected;
    }
    /// <summary>List box control</summary>
    public class ListBox : Control
    {
        public const int MainLayer = 0;
        public const int SelectionLayer = 1;

        #region Event code
        public event EventHandler ContentsChanged;
        public event EventHandler DoubleClick;
        public event EventHandler Selection;
        /// <summary>Raises the contents changed event</summary>
        protected void RaiseContentsChangedEvent(ListBox sender, bool wasTriggeredByUser)
        {
            // Discard events triggered programatically if these types of events haven't been
            // enabled
            if (!Parent.IsUsingNonUserEvents && !wasTriggeredByUser)
                return;

            // Fire the event
            if (ContentsChanged != null)
                ContentsChanged(sender, EventArgs.Empty);
        }
        /// <summary>Raises the double click event</summary>
        protected void RaiseDoubleClickEvent(ListBox sender, bool wasTriggeredByUser)
        {
            // Discard events triggered programatically if these types of events haven't been
            // enabled
            if (!Parent.IsUsingNonUserEvents && !wasTriggeredByUser)
                return;

            // Fire the event
            if (DoubleClick != null)
                DoubleClick(sender, EventArgs.Empty);
        }
        /// <summary>Raises the selection event</summary>
        protected void RaiseSelectionEvent(ListBox sender, bool wasTriggeredByUser)
        {
            // Discard events triggered programatically if these types of events haven't been
            // enabled
            if (!Parent.IsUsingNonUserEvents && !wasTriggeredByUser)
                return;

            // Fire the event
            if (Selection != null)
                Selection(sender, EventArgs.Empty);
        }
        #endregion

        #region Instance data
        private bool isScrollBarInit;
        protected System.Drawing.Rectangle textRect; // Text rendering bound
        protected System.Drawing.Rectangle selectionRect; // Selection box bound
        protected ScrollBar scrollbarControl; 
        protected int scrollWidth;
        protected int border;
        protected int margin;
        protected int textHeight; // Height of a single line of text
        protected int selectedIndex;
        protected int selectedStarted;
        protected bool isDragging;
        protected ListBoxStyle style;
 
        protected ArrayList itemList;
        #endregion

        /// <summary>Create a new list box control</summary>
        public ListBox(Dialog parent) : base(parent)
        {
            // Store control type and parent dialog
            controlType = ControlType.ListBox;
            parentDialog = parent;
            // Create the scrollbar control too
            scrollbarControl = new ScrollBar(parent);

            // Set some default items
            style = ListBoxStyle.SingleSelection;
            scrollWidth = 16;
            selectedIndex = -1;
            selectedStarted = 0;
            isDragging = false;
            margin = 5;
            border = 6;
            textHeight = 0;
            isScrollBarInit = false;

            // Create the item list array
            itemList = new ArrayList();
        }

        /// <summary>Update the rectangles for the list box control</summary>
        protected override void UpdateRectangles()
        {
            // Get bounding box
            base.UpdateRectangles();

            // Calculate the size of the selection rectangle
            selectionRect = boundingBox;
            selectionRect.Width -= scrollWidth;
            selectionRect.Inflate(-border, -border);
            textRect = selectionRect;
            textRect.Inflate(-margin, 0);

            // Update the scroll bars rects too
            scrollbarControl.SetLocation(boundingBox.Right - scrollWidth, boundingBox.Top);
            scrollbarControl.SetSize(scrollWidth, height);
            FontNode fNode = DialogResourceManager.GetGlobalInstance().GetFontNode((int)(elementList[0] as Element).FontIndex);
            if ((fNode != null) && (fNode.Height > 0))
            {
                scrollbarControl.PageSize = (int)(textRect.Height / fNode.Height);

                // The selected item may have been scrolled off the page.
                // Ensure that it is in page again.
                scrollbarControl.ShowItem(selectedIndex);
            }
        }
        /// <summary>Sets the scroll bar width of this control</summary>
        public void SetScrollbarWidth(int width) { scrollWidth = width; UpdateRectangles(); }
        /// <summary>Can this control have focus</summary>
        public override bool CanHaveFocus { get { return (IsVisible && IsEnabled); } }
        /// <summary>Sets the style of the listbox</summary>
        public ListBoxStyle Style { get { return style; } set { style = value; } } 
        /// <summary>Number of items current in the list</summary>
        public int NumberItems { get { return itemList.Count; } }
        /// <summary>Indexer for items in the list</summary>
        public ListBoxItem this[int index]
        {
            get { return (ListBoxItem)itemList[index]; }
        }

        /// <summary>Initialize the scrollbar control here</summary>
        public override void OnInitialize()
        {
            parentDialog.InitializeControl(scrollbarControl);
        }


        /// <summary>Called when the control needs to handle the keyboard</summary>
        public override bool HandleKeyboard(NativeMethods.WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false;

            // Let the scroll bar have a chance to handle it first
            if (scrollbarControl.HandleKeyboard(msg, wParam, lParam))
                return true;

            switch (msg)
            {
                case NativeMethods.WindowMessage.KeyDown:
                {
                    switch((System.Windows.Forms.Keys)wParam.ToInt32())
                    {
                        case System.Windows.Forms.Keys.Up:
                        case System.Windows.Forms.Keys.Down:
                        case System.Windows.Forms.Keys.Next:
                        case System.Windows.Forms.Keys.Prior:
                        case System.Windows.Forms.Keys.Home:
                        case System.Windows.Forms.Keys.End:
                        {
                            // If no items exists, do nothing
                            if (itemList.Count == 0)
                                return true;

                            int oldSelected = selectedIndex;

                            // Adjust selectedIndex
                            switch((System.Windows.Forms.Keys)wParam.ToInt32())
                            {
                                case System.Windows.Forms.Keys.Up: --selectedIndex; break;
                                case System.Windows.Forms.Keys.Down: ++selectedIndex; break;
                                case System.Windows.Forms.Keys.Next: selectedIndex += scrollbarControl.PageSize - 1; break;
                                case System.Windows.Forms.Keys.Prior: selectedIndex -= scrollbarControl.PageSize - 1; break;
                                case System.Windows.Forms.Keys.Home: selectedIndex = 0; break;
                                case System.Windows.Forms.Keys.End: selectedIndex = itemList.Count - 1; break;
                            }

                            // Clamp the item
                            if (selectedIndex < 0)
                                selectedIndex = 0;
                            if (selectedIndex >= itemList.Count)
                                selectedIndex = itemList.Count - 1;

                            // Did the selection change?
                            if (oldSelected != selectedIndex)
                            {
                                if (style == ListBoxStyle.Multiselection)
                                {
                                    // Clear all selection
                                    for(int i = 0; i < itemList.Count; i++)
                                    {
                                        ListBoxItem lbi = (ListBoxItem)itemList[i];
                                        lbi.IsItemSelected = false;
                                        itemList[i] = lbi;
                                    }

                                    // Is shift being held down?
                                    bool shiftDown = ((NativeMethods.GetAsyncKeyState
                                        ((int)System.Windows.Forms.Keys.ShiftKey) & 0x8000) != 0);

                                    if (shiftDown)
                                    {
                                        // Select all items from the start selection to current selected index
                                        int end = Math.Max(selectedStarted, selectedIndex);
                                        for(int i = Math.Min(selectedStarted, selectedIndex); i <= end; ++i)
                                        {
                                            ListBoxItem lbi = (ListBoxItem)itemList[i];
                                            lbi.IsItemSelected = true;
                                            itemList[i] = lbi;
                                        }
                                    }
                                    else
                                    {
                                        ListBoxItem lbi = (ListBoxItem)itemList[selectedIndex];
                                        lbi.IsItemSelected = true;
                                        itemList[selectedIndex] = lbi;

                                        // Update selection start
                                        selectedStarted = selectedIndex;
                                    }

                                }
                                else // Update selection start
                                    selectedStarted = selectedIndex;

                                // adjust scrollbar
                                scrollbarControl.ShowItem(selectedIndex);
                                RaiseSelectionEvent(this, true);
                            }
                        }
                        return true;
                    }
                    break;
                }
            }

            return false;
        }

        /// <summary>Called when the control should handle the mouse</summary>
        public override bool HandleMouse(NativeMethods.WindowMessage msg, System.Drawing.Point pt, IntPtr wParam, IntPtr lParam)
        {
            const int ShiftModifier = 0x0004;
            const int ControlModifier = 0x0008;

            if (!IsEnabled || !IsVisible)
                return false; // Nothing to do

            // First acquire focus
            if (msg == NativeMethods.WindowMessage.LeftButtonDown)
                if (!hasFocus)
                    Dialog.RequestFocus(this);


            // Let the scroll bar handle it first
            if (scrollbarControl.HandleMouse(msg, pt, wParam, lParam))
                return true;

            // Ok, scrollbar didn't handle it, move on
            switch(msg)
            {
                case NativeMethods.WindowMessage.LeftButtonDoubleClick:
                case NativeMethods.WindowMessage.LeftButtonDown:
                {
                    // Check for clicks in the text area
                    if (itemList.Count > 0 && selectionRect.Contains(pt))
                    {
                        // Compute the index of the clicked item
                        int clicked = 0;
                        if (textHeight > 0)
                            clicked = scrollbarControl.TrackPosition + (pt.Y - textRect.Top) / textHeight;
                        else
                            clicked = -1;

                        // Only proceed if the click falls ontop of an item
                        if (clicked >= scrollbarControl.TrackPosition &&
                            clicked < itemList.Count &&
                            clicked < scrollbarControl.TrackPosition + scrollbarControl.PageSize )
                        {
                            Parent.SampleFramework.Window.Capture = true;
                            isDragging = true;

                            // If this is a double click, fire off an event and exit
                            // since the first click would have taken care of the selection
                            // updating.
                            if (msg == NativeMethods.WindowMessage.LeftButtonDoubleClick)
                            {
                                RaiseDoubleClickEvent(this, true);
                                return true;
                            }

                            selectedIndex = clicked;
                            if ( (wParam.ToInt32() & ShiftModifier) == 0)
                                selectedStarted = selectedIndex; // Shift isn't down

                            // If this is a multi-selection listbox, update per-item
                            // selection data.
                            if (style == ListBoxStyle.Multiselection)
                            {
                                // Determine behavior based on the state of Shift and Ctrl
                                ListBoxItem selectedItem = (ListBoxItem)itemList[selectedIndex];
                                if ((wParam.ToInt32() & (ShiftModifier | ControlModifier)) == ControlModifier)
                                {
                                    // Control click, reverse the selection
                                    selectedItem.IsItemSelected = !selectedItem.IsItemSelected;
                                    itemList[selectedIndex] = selectedItem;
                                }
                                else if ((wParam.ToInt32() & (ShiftModifier | ControlModifier)) == ShiftModifier)
                                {
                                    // Shift click. Set the selection for all items
                                    // from last selected item to the current item.
                                    // Clear everything else.
                                    int begin = Math.Min(selectedStarted, selectedIndex);
                                    int end = Math.Max(selectedStarted, selectedIndex);

                                    // Unselect everthing before the beginning
                                    for(int i = 0; i < begin; ++i)
                                    {
                                        ListBoxItem lb = (ListBoxItem)itemList[i];
                                        lb.IsItemSelected = false;
                                        itemList[i] = lb;
                                    }
                                    // unselect everything after the end
                                    for(int i = end + 1; i < itemList.Count; ++i)
                                    {
                                        ListBoxItem lb = (ListBoxItem)itemList[i];
                                        lb.IsItemSelected = false;
                                        itemList[i] = lb;
                                    }

                                    // Select everything between
                                    for(int i = begin; i <= end; ++i)
                                    {
                                        ListBoxItem lb = (ListBoxItem)itemList[i];
                                        lb.IsItemSelected = true;
                                        itemList[i] = lb;
                                    }
                                }
                                else if ((wParam.ToInt32() & (ShiftModifier | ControlModifier)) == (ShiftModifier | ControlModifier))
                                {
                                    // Control-Shift-click.

                                    // The behavior is:
                                    //   Set all items from selectedStarted to selectedIndex to
                                    //     the same state as selectedStarted, not including selectedIndex.
                                    //   Set selectedIndex to selected.
                                    int begin = Math.Min(selectedStarted, selectedIndex);
                                    int end = Math.Max(selectedStarted, selectedIndex);

                                    // The two ends do not need to be set here.
                                    bool isLastSelected = ((ListBoxItem)itemList[selectedStarted]).IsItemSelected;

                                    for (int i = begin + 1; i < end; ++i)
                                    {
                                        ListBoxItem lb = (ListBoxItem)itemList[i];
                                        lb.IsItemSelected = isLastSelected;
                                        itemList[i] = lb;
                                    }

                                    selectedItem.IsItemSelected = true;
                                    itemList[selectedIndex] = selectedItem;

                                    // Restore selectedIndex to the previous value
                                    // This matches the Windows behavior

                                    selectedIndex = selectedStarted;
                                }
                                else
                                {
                                    // Simple click.  Clear all items and select the clicked
                                    // item.
                                    for(int i = 0; i < itemList.Count; ++i)
                                    {
                                        ListBoxItem lb = (ListBoxItem)itemList[i];
                                        lb.IsItemSelected = false;
                                        itemList[i] = lb;
                                    }
                                    selectedItem.IsItemSelected = true;
                                    itemList[selectedIndex] = selectedItem;
                                }
                            } // End of multi-selection case
                            RaiseSelectionEvent(this, true);
                        }
                        return true;
                    }
                    break;
                }
                case NativeMethods.WindowMessage.LeftButtonUp:
                {
                    Parent.SampleFramework.Window.Capture = false;
                    isDragging = false;

                    if (selectedIndex != -1)
                    {
                        // Set all items between selectedStarted and selectedIndex to
                        // the same state as selectedStarted
                        int end = Math.Max(selectedStarted, selectedIndex);
                        for (int i = Math.Min(selectedStarted, selectedIndex) + 1; i < end; ++i)
                        {
                            ListBoxItem lb = (ListBoxItem)itemList[i];
                            lb.IsItemSelected = ((ListBoxItem)itemList[selectedStarted]).IsItemSelected;
                            itemList[i] = lb;
                        }
                        ListBoxItem lbs = (ListBoxItem)itemList[selectedIndex];
                        lbs.IsItemSelected = ((ListBoxItem)itemList[selectedStarted]).IsItemSelected;
                        itemList[selectedIndex] = lbs;

                        // If selectedStarted and selectedIndex are not the same,
                        // the user has dragged the mouse to make a selection.
                        // Notify the application of this.
                        if (selectedIndex != selectedStarted)
                            RaiseSelectionEvent(this, true);
                    }
                    break;
                }
                case NativeMethods.WindowMessage.MouseWheel:
                {
                    int lines = System.Windows.Forms.SystemInformation.MouseWheelScrollLines;
                    int scrollAmount = (int)(NativeMethods.HiWord((uint)wParam.ToInt32()) / Dialog.WheelDelta * lines);
                    scrollbarControl.Scroll(-scrollAmount);
                    break;
                }

                case NativeMethods.WindowMessage.MouseMove:
                {
                    if (isDragging)
                    {
                        // compute the index of the item below the cursor
                        int itemIndex = -1;
                        if (textHeight > 0)
                            itemIndex = scrollbarControl.TrackPosition + (pt.Y - textRect.Top) / textHeight;

                        // Only proceed if the cursor is on top of an item
                        if (itemIndex >= scrollbarControl.TrackPosition &&
                            itemIndex < itemList.Count &&
                            itemIndex < scrollbarControl.TrackPosition + scrollbarControl.PageSize)
                        {
                            selectedIndex = itemIndex;
                            RaiseSelectionEvent(this, true);
                        }
                        else if (itemIndex < scrollbarControl.TrackPosition)
                        {
                            // User drags the mouse above window top
                            scrollbarControl.Scroll(-1);
                            selectedIndex = scrollbarControl.TrackPosition;
                            RaiseSelectionEvent(this, true);
                        }
                        else if (itemIndex >= scrollbarControl.TrackPosition + scrollbarControl.PageSize)
                        {
                            // User drags the mouse below the window bottom
                            scrollbarControl.Scroll(1);
                            selectedIndex = Math.Min(itemList.Count, scrollbarControl.TrackPosition + scrollbarControl.PageSize - 1);
                            RaiseSelectionEvent(this, true);
                        }
                    }
                    break;
                }
            }

            // Didn't handle it
            return false;
        }

        /// <summary>Called when the control should be rendered</summary>
        public override void Render(Device device, float elapsedTime)
        {
            if (!IsVisible)
                return; // Nothing to render
            
            Element e = elementList[ListBox.MainLayer] as Element;

            // Blend current color
            e.TextureColor.Blend(ControlState.Normal, elapsedTime);
            e.FontColor.Blend(ControlState.Normal, elapsedTime);
            
            Element selectedElement = elementList[ListBox.SelectionLayer] as Element;

            // Blend current color
            selectedElement.TextureColor.Blend(ControlState.Normal, elapsedTime);
            selectedElement.FontColor.Blend(ControlState.Normal, elapsedTime);

            parentDialog.DrawSprite(e, boundingBox);

            // Render the text
            if (itemList.Count > 0)
            {
                // Find out the height of a single line of text
                System.Drawing.Rectangle rc = textRect;
                System.Drawing.Rectangle sel = selectionRect;
                rc.Height = (int)(DialogResourceManager.GetGlobalInstance().GetFontNode((int)e.FontIndex).Height);
                textHeight = rc.Height;

                // If we have not initialized the scroll bar page size,
                // do that now.
                if (!isScrollBarInit)
                {
                    if (textHeight > 0)
                        scrollbarControl.PageSize = (int)(textRect.Height / textHeight);
                    else
                        scrollbarControl.PageSize = textRect.Height;

                    isScrollBarInit = true;
                }
                rc.Width = textRect.Width;
                for (int i = scrollbarControl.TrackPosition; i < itemList.Count; ++i)
                {
                    if (rc.Bottom > textRect.Bottom)
                        break;

                    ListBoxItem lb = (ListBoxItem)itemList[i];

                    // Determine if we need to render this item with the
                    // selected element.
                    bool isSelectedStyle = false;

                    if ( (selectedIndex == i) && (style == ListBoxStyle.SingleSelection) )
                        isSelectedStyle = true;
                    else if (style == ListBoxStyle.Multiselection)
                    {
                        if (isDragging && ( ( i >= selectedIndex && i < selectedStarted) || 
                            (i <= selectedIndex && i > selectedStarted) ) )
                        {
                            ListBoxItem selStart = (ListBoxItem)itemList[selectedStarted];
                            isSelectedStyle = selStart.IsItemSelected;
                        }
                        else
                            isSelectedStyle = lb.IsItemSelected;
                    }

                    // Now render the text
                    if (isSelectedStyle)
                    {
                        sel.Location = new System.Drawing.Point(sel.Left, rc.Top); 
                        sel.Height = rc.Height;
                        parentDialog.DrawSprite(selectedElement, sel);
                        parentDialog.DrawText(lb.ItemText, selectedElement, rc);
                    }
                    else
                        parentDialog.DrawText(lb.ItemText, e, rc);

                    rc.Offset(0, textHeight);
                }
            }

            // Render the scrollbar finally
            scrollbarControl.Render(device, elapsedTime);
        }

        
        #region Item Controlling methods
        /// <summary>Adds an item to the list box control</summary>
        public void AddItem(string text, object data)
        {
            if ((text == null) || (text.Length == 0))
                throw new ArgumentNullException("text", "You must pass in a valid item name when adding a new item.");

            // Create a new item and add it
            ListBoxItem newitem = new ListBoxItem();
            newitem.ItemText = text;
            newitem.ItemData = data;
            newitem.IsItemSelected = false;
            itemList.Add(newitem);

            // Update the scrollbar with the new range
            scrollbarControl.SetTrackRange(0, itemList.Count);
        }
        /// <summary>Inserts an item to the list box control</summary>
        public void InsertItem(int index, string text, object data)
        {
            if ((text == null) || (text.Length == 0))
                throw new ArgumentNullException("text", "You must pass in a valid item name when adding a new item.");

            // Create a new item and insert it
            ListBoxItem newitem = new ListBoxItem();
            newitem.ItemText = text;
            newitem.ItemData = data;
            newitem.IsItemSelected = false;
            itemList.Insert(index, newitem);

            // Update the scrollbar with the new range
            scrollbarControl.SetTrackRange(0, itemList.Count);
        }

        /// <summary>Removes an item at a particular index</summary>
        public void RemoveAt(int index)
        {
            // Remove the item
            itemList.RemoveAt(index);

            // Update the scrollbar with the new range
            scrollbarControl.SetTrackRange(0, itemList.Count);

            if (selectedIndex >= itemList.Count)
                selectedIndex = itemList.Count - 1;

            RaiseSelectionEvent(this, true);
        }

        /// <summary>Removes all items from the control</summary>
        public void Clear()
        {
            // clear the list
            itemList.Clear();

            // Update scroll bar and index
            scrollbarControl.SetTrackRange(0, 1);
            selectedIndex = -1;
        }

        /// <summary>
        /// For single-selection listbox, returns the index of the selected item.
        /// For multi-selection, returns the first selected item after the previousSelected position.
        /// To search for the first selected item, the app passes -1 for previousSelected.  For
        /// subsequent searches, the app passes the returned index back to GetSelectedIndex as.
        /// previousSelected.
        /// Returns -1 on error or if no item is selected.
        /// </summary>
        public int GetSelectedIndex(int previousSelected)
        {
            if (previousSelected < -1)
                return -1;

            if (style == ListBoxStyle.Multiselection)
            {
                // Multiple selections enabled.  Search for the next item with the selected flag
                for (int i = previousSelected + 1; i < itemList.Count; ++i)
                {
                    ListBoxItem lbi = (ListBoxItem)itemList[i];
                    if (lbi.IsItemSelected)
                        return i;
                }

                return -1;
            }
            else
            {
                // Single selection
                return selectedIndex;
            }
        }
        /// <summary>Gets the selected item</summary>
        public ListBoxItem GetSelectedItem(int previousSelected)
        {
            return (ListBoxItem)itemList[GetSelectedIndex(previousSelected)];
        }
        /// <summary>Gets the selected item</summary>
        public ListBoxItem GetSelectedItem() { return GetSelectedItem(-1); }

        /// <summary>Sets the border and margin sizes</summary>
        public void SetBorder(int borderSize, int marginSize)
        {
            border = borderSize;
            margin = marginSize;
            UpdateRectangles();
        }

        /// <summary>Selects this item</summary>
        public void SelectItem(int newIndex)
        {
            if (itemList.Count == 0)
                return; // If no items exist there's nothing to do

            int oldSelected = selectedIndex;

            // Select the new item
            selectedIndex = newIndex;

            // Clamp the item
            if (selectedIndex < 0)
                selectedIndex = 0;
            if (selectedIndex > itemList.Count)
                selectedIndex = itemList.Count - 1;

            // Did the selection change?
            if (oldSelected != selectedIndex)
            {
                if (style == ListBoxStyle.Multiselection)
                {
                    ListBoxItem lbi = (ListBoxItem)itemList[selectedIndex];
                    lbi.IsItemSelected = true;
                    itemList[selectedIndex] = lbi;
                }

                // Update selection start
                selectedStarted = selectedIndex;

                // adjust scrollbar
                scrollbarControl.ShowItem(selectedIndex);
            }
            RaiseSelectionEvent(this, true);
        }
        #endregion
    }
    #endregion

    /// <summary>A basic edit box</summary>
    public class EditBox : Control
    {
        #region Element layers
        public const int TextLayer = 0;
        public const int TopLeftBorder = 1;
        public const int TopBorder = 2;
        public const int TopRightBorder = 3;
        public const int LeftBorder = 4;
        public const int RightBorder = 5;
        public const int LowerLeftBorder = 6;
        public const int LowerBorder = 7;
        public const int LowerRightBorder = 8;
        #endregion

        #region Event code
        public event EventHandler Changed;
        public event EventHandler Enter;
        /// <summary>Raises the changed event</summary>
        protected void RaiseChangedEvent(EditBox sender, bool wasTriggeredByUser)
        {
            // Discard events triggered programatically if these types of events haven't been
            // enabled
            if (!Parent.IsUsingNonUserEvents && !wasTriggeredByUser)
                return;

            if (Changed != null)
                Changed(sender, EventArgs.Empty);
        }
        /// <summary>Raises the Enter event</summary>
        protected void RaiseEnterEvent(EditBox sender, bool wasTriggeredByUser)
        {
            // Discard events triggered programatically if these types of events haven't been
            // enabled
            if (!Parent.IsUsingNonUserEvents && !wasTriggeredByUser)
                return;

            if (Enter != null)
                Enter(sender, EventArgs.Empty);
        }
        #endregion

        #region Class Data
        protected System.Windows.Forms.RichTextBox textData; // Text data
        protected int border; // Border of the window
        protected int spacing; // Spacing between the text and the edge of border
        protected System.Drawing.Rectangle textRect; // Bounding rectangle for the text
        protected System.Drawing.Rectangle[] elementRects = new System.Drawing.Rectangle[9];
        protected double blinkTime; // Caret blink time in milliseconds
        protected double lastBlink; // Last timestamp of caret blink
        protected bool isCaretOn; // Flag to indicate whether caret is currently visible
        protected int caretPosition; // Caret position, in characters
        protected bool isInsertMode; // If true, control is in insert mode. Else, overwrite mode.
        protected int firstVisible;  // First visible character in the edit control
        protected ColorValue textColor; // Text color
        protected ColorValue selectedTextColor; // Selected Text color
        protected ColorValue selectedBackColor; // Selected background color
        protected ColorValue caretColor; // Caret color

        // Mouse-specific
        protected bool isMouseDragging; // True to indicate the drag is in progress

        protected static bool isHidingCaret; // If true, we don't render the caret.
        
        #endregion

        #region Simple overrides/properties/methods
        /// <summary>Can the edit box have focus</summary>
        public override bool CanHaveFocus { get { return (IsVisible && IsEnabled); } }
        /// <summary>Update the spacing</summary>
        public void SetSpacing(int space) { spacing = space; UpdateRectangles(); }
        /// <summary>Update the border</summary>
        public void SetBorderWidth(int b) { border = b; UpdateRectangles(); }
        /// <summary>Update the text color</summary>
        public void SetTextColor(ColorValue color) { textColor = color; }
        /// <summary>Update the text selected color</summary>
        public void SetSelectedTextColor(ColorValue color) { selectedTextColor = color; }
        /// <summary>Update the selected background color</summary>
        public void SetSelectedBackColor(ColorValue color) { selectedBackColor = color; }
        /// <summary>Update the caret color</summary>
        public void SetCaretColor(ColorValue color) { caretColor = color; }

        /// <summary>Get or sets the text</summary>
        public string Text { get { return textData.Text; } set { SetText(value, false); } }
        /// <summary>Gets a copy of the text</summary>
        public string GetTextCopy() { return string.Copy(textData.Text); }
        #endregion

        /// <summary>Creates a new edit box control</summary>
        public EditBox(Dialog parent) : base(parent)
        {
            controlType = ControlType.EditBox;
            parentDialog = parent;

            border = 5; // Default border
            spacing = 4; // default spacing
            isCaretOn = true;

            textData = new System.Windows.Forms.RichTextBox();
            // Create the control
            textData.Visible = true;
            textData.Font = new System.Drawing.Font("Arial", 8.0f);
            textData.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            textData.Multiline = false;
            textData.Text = string.Empty;
            textData.MaxLength = ushort.MaxValue; // 65k characters should be plenty
            textData.WordWrap = false;
            // Now create the control
            textData.CreateControl();

            isHidingCaret = false;
            firstVisible = 0;
            blinkTime = NativeMethods.GetCaretBlinkTime() * 0.001f;
            lastBlink = FrameworkTimer.GetAbsoluteTime();
            textColor = new ColorValue(0.06f, 0.06f, 0.06f, 1.0f);
            selectedTextColor = new ColorValue(1.0f, 1.0f, 1.0f, 1.0f);
            selectedBackColor = new ColorValue(0.15f, 0.196f, 0.36f, 1.0f);
            caretColor = new ColorValue(0, 0, 0, 1.0f);
            caretPosition = textData.SelectionStart = 0;
            isInsertMode = true;
            isMouseDragging = false;
        }

        /// <summary>Set the caret to a character position, and adjust the scrolling if necessary</summary>
        protected void PlaceCaret(int pos)
        {
            // Store caret position
            caretPosition = pos;

            // First find the first visible char
            for (int i = 0; i < textData.Text.Length; i++)
            {
                System.Drawing.Point p = textData.GetPositionFromCharIndex(i);
                if (p.X >= 0) 
                {
                    firstVisible = i; // This is the first visible character
                    break;
                }
            }

            // if the new position is smaller than the first visible char 
            // we'll need to scroll
            if (firstVisible > caretPosition)
                firstVisible = caretPosition;
        }

        /// <summary>Clears the edit box</summary>
        public void Clear()
        {
            textData.Text = string.Empty;
            PlaceCaret(0);
            textData.SelectionStart = 0;
        }
        /// <summary>Sets the text for the control</summary>
        public void SetText(string text, bool selected)
        {
            if (text == null)
                text = string.Empty;

            textData.Text = text;
            textData.SelectionStart = text.Length;
            // Move the care to the end of the text
            PlaceCaret(text.Length);
            textData.SelectionStart = (selected) ? 0 : caretPosition;
            FocusText();
        }
        /// <summary>Deletes the text that is currently selected</summary>
        protected void DeleteSelectionText()
        {
            int first = Math.Min(caretPosition, textData.SelectionStart);
            int last = Math.Max(caretPosition, textData.SelectionStart);
            // Update caret and selection
            PlaceCaret(first);
            // Remove the characters
            textData.Text = textData.Text.Remove(first, (last-first));
            textData.SelectionStart = caretPosition;
            FocusText();
        }
        /// <summary>Updates the rectangles used by the control</summary>
        protected override void UpdateRectangles()
        {
            // Get the bounding box first
            base.UpdateRectangles ();

            // Update text rect
            textRect = boundingBox;
            // First inflate by border to compute render rects
            textRect.Inflate(-border, -border);
            
            // Update the render rectangles
            elementRects[0] = textRect;
            elementRects[1] = new System.Drawing.Rectangle(boundingBox.Left, boundingBox.Top, (textRect.Left - boundingBox.Left), (textRect.Top - boundingBox.Top));
            elementRects[2] = new System.Drawing.Rectangle(textRect.Left, boundingBox.Top, textRect.Width, (textRect.Top - boundingBox.Top));
            elementRects[3] = new System.Drawing.Rectangle(textRect.Right, boundingBox.Top, (boundingBox.Right - textRect.Right), (textRect.Top - boundingBox.Top));
            elementRects[4] = new System.Drawing.Rectangle(boundingBox.Left, textRect.Top, (textRect.Left - boundingBox.Left), textRect.Height);
            elementRects[5] = new System.Drawing.Rectangle(textRect.Right, textRect.Top, (boundingBox.Right - textRect.Right), textRect.Height);
            elementRects[6] = new System.Drawing.Rectangle(boundingBox.Left, textRect.Bottom, (textRect.Left - boundingBox.Left), (boundingBox.Bottom - textRect.Bottom));
            elementRects[7] = new System.Drawing.Rectangle(textRect.Left, textRect.Bottom, textRect.Width, (boundingBox.Bottom - textRect.Bottom));
            elementRects[8] = new System.Drawing.Rectangle(textRect.Right, textRect.Bottom, (boundingBox.Right - textRect.Right), (boundingBox.Bottom - textRect.Bottom));            

            // Inflate further by spacing
            textRect.Inflate(-spacing, -spacing);

            // Make the underlying rich text box the same size
            textData.Size = textRect.Size;
        }

        /// <summary>Copy the selected text to the clipboard</summary>
        protected void CopyToClipboard()
        {
            // Copy the selection text to the clipboard
            if (caretPosition != textData.SelectionStart)
            {
                int first = Math.Min(caretPosition, textData.SelectionStart);
                int last = Math.Max(caretPosition, textData.SelectionStart);
                // Set the text to the clipboard
                System.Windows.Forms.Clipboard.SetDataObject(textData.Text.Substring(first, (last-first)));
            }

        }
        /// <summary>Paste the clipboard data to the control</summary>
        protected void PasteFromClipboard()
        {
            // Get the clipboard data
            System.Windows.Forms.IDataObject clipData = System.Windows.Forms.Clipboard.GetDataObject();
            // Does the clipboard have string data?
            if (clipData.GetDataPresent(System.Windows.Forms.DataFormats.StringFormat))
            {
                // Yes, get that data
                string clipString = clipData.GetData(System.Windows.Forms.DataFormats.StringFormat) as string;
                // find any new lines, remove everything after that
                int index;
                if ((index = clipString.IndexOf("\n")) > 0)
                {
                    clipString = clipString.Substring(0, index-1);
                }

                // Insert that into the text data
                textData.Text = textData.Text.Insert(caretPosition, clipString);
                caretPosition += clipString.Length;
                textData.SelectionStart = caretPosition;
                FocusText();
            }
        }
        /// <summary>Reset's the caret blink time</summary>
        protected void ResetCaretBlink()
        {
            isCaretOn = true;
            lastBlink = FrameworkTimer.GetAbsoluteTime();
        }

        /// <summary>Update the caret when focus is in</summary>
        public override void OnFocusIn()
        {
            base.OnFocusIn();
            ResetCaretBlink();
        }

        /// <summary>Updates focus to the backing rich textbox so it updates it's state</summary>
        private void FocusText()
        {
            // Because of a design issue with the rich text box control that is used as 
            // the backing store for this control, the 'scrolling' mechanism built into
            // the control will only work if the control has focus.  Setting focus to the 
            // control here would work, but would cause a bad 'flicker' of the application.

            // Therefore, the automatic horizontal scrolling is turned off by default.  To 
            // enable it turn this define on.
#if (SCROLL_CORRECTLY)
            NativeMethods.SetFocus(textData.Handle);
            NativeMethods.SetFocus(Parent.SampleFramework.Window);
#endif
        }

        /// <summary>Handle keyboard input to the edit box</summary>
        public override bool HandleKeyboard(Microsoft.Samples.DirectX.UtilityToolkit.NativeMethods.WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false;

            // Default to not handling the message
            bool isHandled = false;
            if (msg == NativeMethods.WindowMessage.KeyDown)
            {
                switch((System.Windows.Forms.Keys)wParam.ToInt32())
                {
                    case System.Windows.Forms.Keys.End:
                    case System.Windows.Forms.Keys.Home:
                        // Move the caret
                        if (wParam.ToInt32() == (int)System.Windows.Forms.Keys.End)
                            PlaceCaret(textData.Text.Length);
                        else
                            PlaceCaret(0);
                        if (!NativeMethods.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
                        {
                            // Shift is not down. Update selection start along with caret
                            textData.SelectionStart = caretPosition;
                            FocusText();
                        }

                        ResetCaretBlink();
                        isHandled = true;
                        break;
                    case System.Windows.Forms.Keys.Insert:
                        if (NativeMethods.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                        {
                            // Control insert -> Copy to clipboard
                            CopyToClipboard();
                        }
                        else if (NativeMethods.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
                        {
                            // Shift insert -> Paste from clipboard
                            PasteFromClipboard();
                        }
                        else
                        {
                            // Toggle insert mode
                            isInsertMode = !isInsertMode;
                        }
                        break;
                    case System.Windows.Forms.Keys.Delete:
                        // Check to see if there is a text selection
                        if (caretPosition != textData.SelectionStart)
                        {
                            DeleteSelectionText();
                            RaiseChangedEvent(this, true);
                        }
                        else
                        {
                            if (caretPosition < textData.Text.Length)
                            {
                                // Deleting one character
                                textData.Text = textData.Text.Remove(caretPosition, 1);
                                RaiseChangedEvent(this, true);
                            }
                        }
                        ResetCaretBlink();
                        isHandled = true;
                        break;

                    case System.Windows.Forms.Keys.Left:
                        if (NativeMethods.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                        {
                            // Control is down. Move the caret to a new item
                            // instead of a character.
                        }
                        else if (caretPosition > 0)
                            PlaceCaret(caretPosition - 1); // Move one to the left

                        if (!NativeMethods.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
                        {
                            // Shift is not down. Update selection
                            // start along with the caret.
                            textData.SelectionStart = caretPosition;
                            FocusText();
                        }
                        ResetCaretBlink();
                        isHandled = true;
                        break;

                    case System.Windows.Forms.Keys.Right:
                        if (NativeMethods.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                        {
                            // Control is down. Move the caret to a new item
                            // instead of a character.
                        }
                        else if (caretPosition < textData.Text.Length)
                            PlaceCaret(caretPosition + 1); // Move one to the left
                        if (!NativeMethods.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
                        {
                            // Shift is not down. Update selection
                            // start along with the caret.
                            textData.SelectionStart = caretPosition;
                            FocusText();
                        }
                        ResetCaretBlink();
                        isHandled = true;
                        break;

                    case System.Windows.Forms.Keys.Up:
                    case System.Windows.Forms.Keys.Down:
                        // Trap up and down arrows so that the dialog
                        // does not switch focus to another control.
                        isHandled = true;
                        break;

                    default:
                        // Let the application handle escape
                        isHandled = ((System.Windows.Forms.Keys)wParam.ToInt32()) == System.Windows.Forms.Keys.Escape;
                        break;
                }
            }

            return isHandled;
        }

        /// <summary>Handle mouse messages</summary>
        public override bool HandleMouse(NativeMethods.WindowMessage msg, System.Drawing.Point pt, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false;

            // We need a new point
            System.Drawing.Point p = pt;
            p.X -= textRect.Left;
            p.Y -= textRect.Top;

            switch(msg)
            {
                case NativeMethods.WindowMessage.LeftButtonDown:
                case NativeMethods.WindowMessage.LeftButtonDoubleClick:
                    // Get focus first
                    if (!hasFocus)
                        Dialog.RequestFocus(this);

                    if (!ContainsPoint(pt))
                        return false;

                    isMouseDragging = true;
                    Parent.SampleFramework.Window.Capture = true;
                    // Determine the character corresponding to the coordinates
                    int index = textData.GetCharIndexFromPosition(p);

                    System.Drawing.Point startPosition = textData.GetPositionFromCharIndex(index);

                    if (p.X > startPosition.X && index < textData.Text.Length)
                        PlaceCaret(index + 1);
                    else
                        PlaceCaret(index);
                    
                    textData.SelectionStart = caretPosition;
                    FocusText();
                    ResetCaretBlink();
                    return true;

                case NativeMethods.WindowMessage.LeftButtonUp:
                    Parent.SampleFramework.Window.Capture = false;
                    isMouseDragging = false;
                    break;
                case NativeMethods.WindowMessage.MouseMove:
                    if (isMouseDragging)
                    {
                        // Determine the character corresponding to the coordinates
                        int dragIndex = textData.GetCharIndexFromPosition(p);

                        if (dragIndex < textData.Text.Length)
                            PlaceCaret(dragIndex + 1);
                        else
                            PlaceCaret(dragIndex);
                    }
                    break;
            }
            return false;
        }

        /// <summary>Handle all other messages</summary>
        public override bool MsgProc(IntPtr hWnd, NativeMethods.WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false;

            if (msg == NativeMethods.WindowMessage.Character)
            {
                int charKey = wParam.ToInt32();
                switch(charKey)
                {
                    case (int)System.Windows.Forms.Keys.Back:
                    {
                        // If there's a selection, treat this
                        // like a delete key.
                        if (caretPosition != textData.SelectionStart)
                        {
                            DeleteSelectionText();
                            RaiseChangedEvent(this, true);
                        }
                        else if (caretPosition > 0)
                        {
                            // Move the caret and delete the char
                            textData.Text = textData.Text.Remove(caretPosition - 1, 1);
                            PlaceCaret(caretPosition - 1);
                            textData.SelectionStart = caretPosition;
                            FocusText();
                            RaiseChangedEvent(this, true);
                        }

                        ResetCaretBlink();
                        break;
                    }
                    case 24: // Ctrl-X Cut
                    case (int)System.Windows.Forms.Keys.Cancel: // Ctrl-C Copy
                    {
                        CopyToClipboard();

                        // If the key is Ctrl-X, delete the selection too.
                        if (charKey == 24)
                        {
                            DeleteSelectionText();
                            RaiseChangedEvent(this, true);
                        }

                        break;
                    }

                    // Ctrl-V Paste
                    case 22:
                    {
                        PasteFromClipboard();
                        RaiseChangedEvent(this, true);
                        break;
                    }
                    case (int)System.Windows.Forms.Keys.Return:
                        // Invoke the event when the user presses Enter.
                        RaiseEnterEvent(this, true);
                        break;

                    // Ctrl-A Select All
                    case 1:
                    {
                        if (textData.SelectionStart == caretPosition)
                        {
                            textData.SelectionStart = 0;
                            PlaceCaret(textData.Text.Length);
                        }
                        break;
                    }

                    // Junk characters we don't want in the string
                    case 26:  // Ctrl Z
                    case 2:   // Ctrl B
                    case 14:  // Ctrl N
                    case 19:  // Ctrl S
                    case 4:   // Ctrl D
                    case 6:   // Ctrl F
                    case 7:   // Ctrl G
                    case 10:  // Ctrl J
                    case 11:  // Ctrl K
                    case 12:  // Ctrl L
                    case 17:  // Ctrl Q
                    case 23:  // Ctrl W
                    case 5:   // Ctrl E
                    case 18:  // Ctrl R
                    case 20:  // Ctrl T
                    case 25:  // Ctrl Y
                    case 21:  // Ctrl U
                    case 9:   // Ctrl I
                    case 15:  // Ctrl O
                    case 16:  // Ctrl P
                    case 27:  // Ctrl [
                    case 29:  // Ctrl ]
                    case 28:  // Ctrl \ 
                        break;
                    
                    default:
                    {
                        // If there's a selection and the user
                        // starts to type, the selection should
                        // be deleted.
                        if (caretPosition != textData.SelectionStart)
                        {
                            DeleteSelectionText();
                        }
                        // If we are in overwrite mode and there is already
                        // a char at the caret's position, simply replace it.
                        // Otherwise, we insert the char as normal.
                        if (!isInsertMode && caretPosition < textData.Text.Length)
                        {
                            // This isn't the most efficient way to do this, but it's simple
                            // and shows the correct behavior
                            char[] charData = textData.Text.ToCharArray();
                            charData[caretPosition] = (char)wParam.ToInt32();
                            textData.Text = new string(charData);
                        }
                        else
                        {
                            // Insert the char
                            char c = (char)wParam.ToInt32();
                            textData.Text = textData.Text.Insert(caretPosition, c.ToString());
                        }

                        // Move the caret and selection position now
                        PlaceCaret(caretPosition + 1);
                        textData.SelectionStart = caretPosition;
                        FocusText();

                        ResetCaretBlink();
                        RaiseChangedEvent(this, true);
                        break;
                    }
                }
            }
            return false;
        }


        /// <summary>Render the control</summary>
        public override void Render(Device device, float elapsedTime)
        {
            if (!IsVisible)
                return; // Nothing to render

            // Render the control graphics
            for (int i = 0; i <= LowerRightBorder; ++i)
            {
                Element e = elementList[i] as Element;
                e.TextureColor.Blend(ControlState.Normal,elapsedTime);
                parentDialog.DrawSprite(e, elementRects[i]);
            }
            //
            // Compute the X coordinates of the first visible character.
            //
            int xFirst = textData.GetPositionFromCharIndex(firstVisible).X;
            int xCaret = textData.GetPositionFromCharIndex(caretPosition).X;
            int xSel;

            if (caretPosition != textData.SelectionStart)
                xSel = textData.GetPositionFromCharIndex(textData.SelectionStart).X;
            else
                xSel = xCaret;

            // Render the selection rectangle
            System.Drawing.Rectangle selRect = System.Drawing.Rectangle.Empty;
            if (caretPosition != textData.SelectionStart)
            {
                int selLeft = xCaret, selRight = xSel;
                // Swap if left is beigger than right
                if (selLeft > selRight)
                {
                    int temp = selLeft;
                    selLeft = selRight;
                    selRight = temp;
                }
                selRect = System.Drawing.Rectangle.FromLTRB(
                    selLeft, textRect.Top, selRight, textRect.Bottom);
                selRect.Offset(textRect.Left - xFirst, 0);
                selRect.Intersect(textRect);
                Parent.DrawRectangle(selRect, selectedBackColor);
            }

            // Render the text
            Element textElement = elementList[TextLayer] as Element;
            textElement.FontColor.Current = textColor;
            parentDialog.DrawText(textData.Text.Substring(firstVisible), textElement, textRect);
            
            // Render the selected text
            if (caretPosition != textData.SelectionStart)
            {
                int firstToRender = Math.Max(firstVisible, Math.Min(textData.SelectionStart, caretPosition));
                int numToRender = Math.Max(textData.SelectionStart, caretPosition) - firstToRender;
                textElement.FontColor.Current = selectedTextColor;
                parentDialog.DrawText(textData.Text.Substring(firstToRender, numToRender), textElement, selRect);
            }

            //
            // Blink the caret
            //
            if(FrameworkTimer.GetAbsoluteTime() - lastBlink >= blinkTime)
            {
                isCaretOn = !isCaretOn;
                lastBlink = FrameworkTimer.GetAbsoluteTime();
            }

            //
            // Render the caret if this control has the focus
            //
            if( hasFocus && isCaretOn && !isHidingCaret )
            {
                // Start the rectangle with insert mode caret
                System.Drawing.Rectangle caretRect = textRect;
                caretRect.Width = 2;
                caretRect.Location = new System.Drawing.Point(
                    caretRect.Left - xFirst + xCaret -1, 
                    caretRect.Top);
                
                // If we are in overwrite mode, adjust the caret rectangle
                // to fill the entire character.
                if (!isInsertMode)
                {
                    // Obtain the X coord of the current character
                    caretRect.Width = 4;
                }

                parentDialog.DrawRectangle(caretRect, caretColor);
            }

        }
    }
}