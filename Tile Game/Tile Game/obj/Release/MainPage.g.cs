﻿

#pragma checksum "C:\Users\Richard\Documents\GitHub\WAMP_SlidingPuzzle\Tile Game\Tile Game\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "1860AE74D82D7A0BE433ED3F7EFB259D"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tile_Game
{
    partial class MainPage : global::Windows.UI.Xaml.Controls.Page, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 8 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).PointerReleased += this.Page_PointerReleased;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 10 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).PointerReleased += this.MainMenuGrid_PointerReleased;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 29 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).PointerMoved += this.GameGrid_PointerMoved;
                 #line default
                 #line hidden
                #line 29 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).PointerReleased += this.GameGrid_PointerReleased;
                 #line default
                 #line hidden
                #line 29 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).PointerPressed += this.GameGrid_PointerPressed;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 145 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_Click;
                 #line default
                 #line hidden
                break;
            case 5:
                #line 148 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.submitNamePrompt_Click;
                 #line default
                 #line hidden
                break;
            case 6:
                #line 159 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.btnLoadPicture_Click;
                 #line default
                 #line hidden
                break;
            case 7:
                #line 160 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.btnLoadCamera_Click;
                 #line default
                 #line hidden
                break;
            case 8:
                #line 161 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.btnRandomizeButtom_Click;
                 #line default
                 #line hidden
                break;
            case 9:
                #line 162 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.btnSolveButton_Click;
                 #line default
                 #line hidden
                break;
            case 10:
                #line 163 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.btnToggleNumber_Click;
                 #line default
                 #line hidden
                break;
            case 11:
                #line 164 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.btnViewLeaderboard_Click;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


