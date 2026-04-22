using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace m0.UIWpf.Controls
{
    public class WebView2UrlControl : Grid, IDisposable
    {
        private readonly object webView2Control;
        private readonly Action<object> disposeAction;

        public WebView2UrlControl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("Url can not be null or empty.", nameof(url));
            }

            Uri targetUri = new Uri(url, UriKind.Absolute);

            FrameworkElement webViewElement = CreateWebViewElement(out this.webView2Control, out this.disposeAction);
            this.Children.Add(webViewElement);

            Navigate(targetUri);
        }

        private FrameworkElement CreateWebViewElement(out object createdControl, out Action<object> createdDisposeAction)
        {
            Type webView2Type = Type.GetType("Microsoft.Web.WebView2.Wpf.WebView2, Microsoft.Web.WebView2.Wpf", false);
            if (webView2Type == null)
            {
                throw new InvalidOperationException("WebView2 control type was not found. Add Microsoft.Web.WebView2 package.");
            }

            object instance = Activator.CreateInstance(webView2Type);
            if (instance == null)
            {
                throw new InvalidOperationException("Can not create WebView2 control instance.");
            }

            FrameworkElement frameworkElement = instance as FrameworkElement;
            if (frameworkElement == null)
            {
                throw new InvalidOperationException("WebView2 instance is not a WPF FrameworkElement.");
            }

            createdControl = instance;

            IDisposable disposable = instance as IDisposable;
            if (disposable != null)
            {
                createdDisposeAction = control => ((IDisposable)control).Dispose();
            }
            else
            {
                createdDisposeAction = null;
            }

            return frameworkElement;
        }

        private void Navigate(Uri targetUri)
        {
            PropertyInfo sourceProperty = this.webView2Control.GetType().GetProperty("Source", BindingFlags.Instance | BindingFlags.Public);
            if (sourceProperty == null || !sourceProperty.CanWrite)
            {
                throw new InvalidOperationException("Can not set Source property on WebView2 control.");
            }

            sourceProperty.SetValue(this.webView2Control, targetUri);
        }

        public void Dispose()
        {
            if (this.disposeAction != null)
            {
                this.disposeAction(this.webView2Control);
            }
        }
    }
}
