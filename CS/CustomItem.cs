using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace CS
{
     class CustomItem : INotifyPropertyChanged
     {
          public int Id { get; private set; }

          private AuditStatusEnum _auditStatus = AuditStatusEnum.Unprocessed;
          public AuditStatusEnum AuditStatus
          {
               get
               {

                    return _auditStatus;
               }
               set
               {
                    _auditStatus = value;
                    NotifyPropertyChanged();
               }
          }

          private bool _isChecked = false;
          public bool IsChecked
          {
               get
               {
                    return _isChecked;
               }
               set
               {
                    _isChecked = value;
                    NotifyPropertyChanged();
               }
          }
          public Dictionary<string, string> Properties { get; private set; }
          public CustomItem(int id, Dictionary<string, string> properties)
          {
               Id = id;
               Properties = properties;
          }

          public event PropertyChangedEventHandler PropertyChanged;
          private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
          {
               PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
          }
          public override string ToString()
          {
               StringBuilder sb = new();
               sb.Append("<custom_item>\n");
               foreach (var key in Properties.Keys)
               {
                    sb.Append(key + ":" + Properties[key] + "\n");
               }
               sb.Append("</custom_item>\n");
               return sb.ToString();
          }
     }
}
