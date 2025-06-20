using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace m0.UIWpf.Forms
{
    /// <summary>
    /// Interaction logic for Transaction.xaml
    /// </summary>
    public partial class Transaction : Window
    {
        MinusZero instance;

        public Transaction(Window owner)
        {
            this.Owner = owner;

            InitializeComponent();

            instance = MinusZero.Instance;


            foreach(IStore store in instance.Stores)
            {
                ListViewItem lvt = new ListViewItem();
                lvt.Content = store.Identifier;
                lvt.Tag = store;

                stores.Items.Add(lvt);
            }


            ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Universe_start_Click(object sender, RoutedEventArgs e)
        {
            instance.StartTransaction();
        }

        private void Universe_commit_Click(object sender, RoutedEventArgs e)
        {
            instance.CommitTransaction();
        }

        private void Universe_rollback_Click(object sender, RoutedEventArgs e)
        {
            instance.RollbackTransaction();
        }

        private void Universe_refresh_Click(object sender, RoutedEventArgs e)
        {
            instance.Refresh();
        }

        IStore selectedStore = null;

        private void Stores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewItem lvt = (ListViewItem)stores.SelectedItem;

            if (lvt == null)
            {
                selectedStore = null;

                store_begin.IsEnabled = false;
                store_commit.IsEnabled = false;
                store_refresh.IsEnabled = false;
                store_rollback.IsEnabled = false;
            }
            else { 
                selectedStore = (IStore)lvt.Tag;

                store_begin.IsEnabled = true;
                store_commit.IsEnabled = true;
                store_refresh.IsEnabled = true;
                store_rollback.IsEnabled = true;
            }
        }

        private void Store_begin_Click(object sender, RoutedEventArgs e)
        {
            if (selectedStore != null)
                selectedStore.StartTransaction();
        }

        private void Store_commit_Click(object sender, RoutedEventArgs e)
        {
            if (selectedStore != null)
                selectedStore.CommitTransaction();
        }

        private void Store_rollback_Click(object sender, RoutedEventArgs e)
        {
            if (selectedStore != null)
                selectedStore.RollbackTransaction();

        }

        private void Store_refresh_Click(object sender, RoutedEventArgs e)
        {
            if (selectedStore != null)
                selectedStore.Refresh();
        }
    }
}
