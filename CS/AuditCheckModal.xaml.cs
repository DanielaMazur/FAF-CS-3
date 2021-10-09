using System.Collections.ObjectModel;
using System.Windows;

namespace CS
{
     /// <summary>
     /// Interaction logic for Window1.xaml
     /// </summary>
     public partial class AuditCheckModal : Window
     {
          public AuditCheckModal()
          {
                InitializeComponent();
          }

          internal void SetResources(ObservableCollection<AuditCheck> auditCheck)
          {
               Resources["AuditCheckResultList"] = auditCheck;
          }

          private void okButton_Click(object sender, RoutedEventArgs e)
          {
               DialogResult = true;
          }
     }
}
