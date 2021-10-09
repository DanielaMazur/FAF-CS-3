namespace CS
{
     class AuditCheck
     {
          public bool IsSuccessfull { get; set; }
          public CustomItem CustomItem { get; set; }

          public override string ToString()
          {
               CustomItem.Properties.TryGetValue("description", out string description);
               return $"Description: {description}\n Is Successful: {IsSuccessfull}";
          }
     }
}
