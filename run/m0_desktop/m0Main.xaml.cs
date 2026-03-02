using m0.Foundation;
using m0.Graph;
using m0.Store;
using m0.Store.Json;
using m0.UIWpf;
using m0.UIWpf.Commands;
using m0.UIWpf.Dialog;
using m0.UIWpf.Visualisers;
using m0.UIWpf.Visualisers.Helper;
using m0.Util;
using m0.ZeroTypes;
using m0.ZeroTypes.UX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace m0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class m0Main : Window, IUserInteraction
    {
        public bool HasBeenShown = false;

        public static m0Main Instance;

        public static TreeVisualiser mainTree;

        public m0Main()
        {
            Instance = this;

            InitializeComponent();

            this.Loaded += new RoutedEventHandler(m0Main_Loaded);            

            this_static = this;            
        }

        public void Init()
        {
            IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(null, null, MinusZero.Instance.Root);

            mainTree = new TreeVisualiser(baseEdgeVertex, null, false);

            this.root.Content = mainTree;
        }


        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        // SYSTEM MENU BEG

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        #region Win32 API Stuff

        // Define the Win32 API methods we are going to use
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);

        /// Define our Constants we will use
        public const Int32 WM_SYSCOMMAND = 0x112;
        public const Int32 MF_SEPARATOR = 0x800;
        public const Int32 MF_BYPOSITION = 0x400;
        public const Int32 MF_STRING = 0x0;

        #endregion

        // The constants we'll use to identify our custom system menu items
        public const Int32 _TransactionSysMenuID = 1000;
        public const Int32 _AboutSysMenuID = 1001;

        /// <summary>
        /// This is the Win32 Interop Handle for this Window
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                return new WindowInteropHelper(this).Handle;
            }
        }

        private void m0Main_Loaded(object sender, RoutedEventArgs e)
        {
            /// Get the Handle for the Forms System Menu
            IntPtr systemMenuHandle = GetSystemMenu(this.Handle, false);            
            
            InsertMenu(systemMenuHandle, 0, MF_BYPOSITION, _TransactionSysMenuID, "Transactions");
            InsertMenu(systemMenuHandle, 1, MF_BYPOSITION, _AboutSysMenuID, "About");
            InsertMenu(systemMenuHandle, 2, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty); // <-- Add a menu seperator

            // Attach our WndProc handler to this Window
            HwndSource source = HwndSource.FromHwnd(this.Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            //

            MinusZero.Instance.Initialize_AfterUXInitialized();

            HasBeenShown = true;
        }

        static m0Main this_static;

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Check if a System Command has been executed
            if (msg == WM_SYSCOMMAND)
            {
                // Execute the appropriate code for the System Menu item that was clicked
                switch (wParam.ToInt32())
                {
                    case _TransactionSysMenuID:

                        m0.UIWpf.Forms.Transaction transaction = new UIWpf.Forms.Transaction(this_static);

                        handled = true;
                        break;
                    case _AboutSysMenuID:

                        m0.UIWpf.Forms.About about = new UIWpf.Forms.About(this_static);

                        handled = true;

                        break;
                }
            }

            return IntPtr.Zero;
        }

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        // SYSTEM MENU END

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        

        public void ShowContent(object obj)
        {
            _ShowContent(obj);
        }        

        protected LayoutAnchorable _ShowContent(object obj)
        {            
            LayoutAnchorable a = new LayoutAnchorable();

            a.CanClose = true;

            if (obj is IPlatformClass)
            {
                IPlatformClass pc=(IPlatformClass)obj;

                if (pc.Vertex == null)
                    return null;

                if (pc.Vertex.Get(false, @"BaseEdge:\To:")!=null&&pc.Vertex.Get(false, @"BaseEdge:\To:").Value != null&&(!GeneralUtil.CompareStrings(pc.Vertex.Get(false, @"BaseEdge:\To:").Value,"")))
                    a.Title = pc.Vertex.Get(false, @"BaseEdge:\To:").Value.ToString();
                else
                    a.Title = (string)pc.Vertex.Value;

                PlatformClassSimpleWrapper pcsw = new PlatformClassSimpleWrapper();

                pcsw.SetContent(pc);

                a.Content = pcsw;

                a.IsVisibleChanged += pcsw.HideEventHandler;

                //a.Closing +=pcsw.ClosedEventHandler;
                a.Closed += pcsw.ClosedEventHandler;

                // not work - to focus
                //System.Windows.Input.Keyboard.Focus((IInputElement)pc);

                this.Pane.Children.Add(a);
                
                pcsw.IsIntialising = true;

                //try
                //{

                //if(a.ContentId != null) // need to find exception source
                    a.Hide(); // this works

                //}catch(Exception e)
                //{ } // sometimes it fails

                a.Show(); // for getting focus

                pcsw.IsIntialising = false;
            }else{
                a.Title = obj.ToString();
                a.Content = obj;

                this.Pane.Children.Add(a);

                a.Hide(); // this works
                a.Show(); // for getting focus
            }
            
            //a.AddToLayout(this.dockingManager, AnchorableShowStrategy.Most); 
            // maybe for later use

            return a;
        }

        public int DialogWindowDefaultWidth_Micro = 300;
        public int DialogWindowDefaultHeight_Micro = 210;

        public int DialogWindowDefaultWidth_Small=300;
        public int DialogWindowDefaultHeight_Small = 325;

        public int DialogWindowDefaultWidth_Medium = 300*2;
        public int DialogWindowDefaultHeight_Medium = (int)(325*1.5);

        public int DialogWindowDefaultWidth_Large = 750;
        public int DialogWindowDefaultHeight_Large = 550;

        public void ShowContentFloating(object obj, FloatingWindowSize size)
        {
            int width=0, height=0;

            switch (size)
            {
                case FloatingWindowSize.Micro:
                    width = DialogWindowDefaultWidth_Micro;
                    height = DialogWindowDefaultHeight_Micro;
                    break;

                case FloatingWindowSize.Small:
                    width = DialogWindowDefaultWidth_Small;
                    height = DialogWindowDefaultHeight_Small;
                    break;

                case FloatingWindowSize.Medium:
                    width = DialogWindowDefaultWidth_Medium;
                    height = DialogWindowDefaultHeight_Medium;
                    break;

                case FloatingWindowSize.Large:
                    width = DialogWindowDefaultWidth_Large;
                    height = DialogWindowDefaultHeight_Large;
                    break;
            }

            ShowContentFloating_withSize(obj, width, height);
        }

        public void ShowContentFloating_withSize(object obj, double DialogWindowWidth, double DialogWindowHeight){            
            LayoutAnchorable a=_ShowContent(obj);
            

            a.FloatingTop = this.Top + (this.Height / 2) - (Math.Min(this.Height,DialogWindowHeight) / 2);
            a.FloatingLeft = this.Left + (this.Width / 2) - (Math.Min(this.Width,DialogWindowWidth) / 2);

            a.FloatingWidth = DialogWindowWidth;
            a.FloatingHeight = DialogWindowHeight;            

            a.Float();
        }

        public void CloseWindowByContent(object obj)
        {
            LayoutContent layoutContent = dockingManager.Layout.ActiveContent;

            layoutContent.Close();
        }

        public void InteractionOutput(string info)
        {
            m0.UIWpf.Dialog.InfoWindow i = new UIWpf.Dialog.InfoWindow();

            i.Text = info;

            i.ShowDialog();
        }

        public void InteractionOutputException(IVertex exception)
        {
            m0.UIWpf.Dialog.ExceptionInfoWindow i = new UIWpf.Dialog.ExceptionInfoWindow();

            if (IsLoaded)
                i.Owner = this;

            if (exception.Get(false, "Type:") != null)
                i.Type = exception.Get(false, "Type:").Value.ToString();

            if (exception.Get(false, "Where:")!=null)
                i.Where = exception.Get(false, "Where:").Value.ToString();

            if (exception.Get(false, "What:") != null)
                i.What = exception.Get(false, "What:").Value.ToString();            

            i.ShowDialog();
        }

        public Point? PositionForUserInteraction = null;

        public IVertex InteractionSelect(IVertex info, IList<IEdge> options, bool firstSelected)
        {
            SelectWindow d = new SelectWindow(info, options, firstSelected, PositionForUserInteraction);

            PositionForUserInteraction = null;

            return d.SelectedOption;
        }

        public IVertex InteractionSelectButton(IVertex info, IList<IEdge> options)
        {
            SelectWindowButton d = new SelectWindowButton(info, options, PositionForUserInteraction);

            PositionForUserInteraction = null;

            return d.SelectedOption;
        }

        public void EditEdge(IVertex baseVertex)
        {
            ShowContentFloating_withSize( new Edit(baseVertex), 500, 550);
        }

        public void OpenDefaultVisualiser(IVertex baseVertex, bool isFloating)
        {
            BaseCommands.OpenDefaultVisualiser(baseVertex, isFloating);
        }

        public void OpenVisualiser(IVertex baseVertex, IVertex inputVertex, bool isFloating)
        {
            BaseCommands.OpenVisualiser(baseVertex, inputVertex, isFloating);
        }

        public void OpenCodeVisualiser(IVertex baseVertex, bool isFloating)
        {
            IVertex codeVis = MinusZero.Instance.root.Get(false, @"System\Meta\Visualiser\Code");

            BaseCommands.OpenVisualiser(baseVertex, codeVis, true);
        }

        public void OpenFormVisualiser(IVertex baseVertex, bool isFloating)
        {
            BaseCommands.OpenFormVisualiser(baseVertex, isFloating);
        }

        public string InteractionInput(String question)
        {
            string answer = new StringQuestionWindow(question, PositionForUserInteraction).Answer;

            PositionForUserInteraction = null;

            return answer;
        }

        public void UserInteractionInitialize()
        {
            UIWpf.WpfUtil.InitializeUIWpf();
        }

        public void UserInteractionFinalize()
        {
            VisualisersList.RemoveAllVisualisers();
        }

        public bool TypedEdge_Get_Test(Type[] interfacesInToCreateType)
        {
            if (interfacesInToCreateType.Contains(typeof(IVisualiser)) && !interfacesInToCreateType.Contains(typeof(IUXVisualiser)))
                return true;
            else
                return false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m0.MinusZero.Instance.Dispose();
        }
    }
}
