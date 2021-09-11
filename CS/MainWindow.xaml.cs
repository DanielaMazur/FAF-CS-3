using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CS
{
     /// <summary>
     /// Interaction logic for MainWindow.xaml
     /// </summary>
     public partial class MainWindow : Window
     {
          private string JSONFileData { get; set; }
          public MainWindow()
          {
               InitializeComponent();
          }

          private void ButtonUpload_Click(object sender, RoutedEventArgs e)
          {
               OpenFileDialog fileDialog = new();
               fileDialog.DefaultExt = ".audit";
               var dialogResult = fileDialog.ShowDialog();

               if (dialogResult.HasValue && dialogResult.Value)
               {
                    try
                    {
                         using var sr = new StreamReader(fileDialog.FileName);
                         FileTextBlock.Text = sr.ReadToEnd();
                         ConvertToJson.IsEnabled = true;
                    }
                    catch (IOException error)
                    {
                         MessageBox.Show("The file could not be read: " + error.Message, "Error");
                    }
               }
          }

          private void ButtonConvertToJson_Click(object sender, RoutedEventArgs e)
          {
               var textToBeConverted = FileTextBlock.Text;
               var custom_items = new List<Dictionary<string, string>>();

               bool isCustmItemParsingStarted = false;
               KeyValuePair<string, string> multilineStringKeyValuePair = new(null, null);

               Dictionary<string, string> activeCutsomItem = new();

               TreeViewItem activeTreeKey = new();

               TreeViewItem DefaultTreeViewItem = new();
               DefaultTreeViewItem.Header = "custom_items";
               TreeView.Items.Add(DefaultTreeViewItem);

               foreach (var textLine in textToBeConverted.Split("\n"))
               {
                    if (textLine.StartsWith("#")) continue;

                    if (textLine.Contains("<custom_item>"))
                    {
                         isCustmItemParsingStarted = true;
                         continue;
                    }

                    if (textLine.Contains("</custom_item>"))
                    {
                         //create a new key which is the index of the item in the collection
                         activeTreeKey = new();
                         activeTreeKey.Header = custom_items.Count;
                         DefaultTreeViewItem.Items.Add(activeTreeKey);

                         custom_items.Add(activeCutsomItem);

                         activeCutsomItem = new();
                         isCustmItemParsingStarted = false;
                         continue;
                    }

                    if (multilineStringKeyValuePair.Key != null)
                    {
                         multilineStringKeyValuePair = new(multilineStringKeyValuePair.Key, multilineStringKeyValuePair.Value + textLine);
                         bool isLastLineInStirng = textLine.Trim().EndsWith('"');

                         if (isLastLineInStirng)
                         {
                              activeCutsomItem.Add(multilineStringKeyValuePair.Key, multilineStringKeyValuePair.Value.Replace("\"", ""));

                              //set key value pair for the created collection keys
                              TreeViewItem newValue = new();
                              newValue.Header = $"{multilineStringKeyValuePair.Key} : {multilineStringKeyValuePair.Value}";
                              activeTreeKey.Items.Add(newValue);

                              multilineStringKeyValuePair = new(null, null);
                         }
                         continue;
                    }

                    if (isCustmItemParsingStarted)
                    {
                         var keyValuePair = textLine.Split(':', 2);
                         var key = keyValuePair[0].Trim();
                         var value = keyValuePair[1].Trim();
                         var isValueStirng = value.StartsWith('"');
                         var isValueFinishedInCurrentLine = value.EndsWith('"');
                         if (isValueStirng && !isValueFinishedInCurrentLine)
                         {
                              multilineStringKeyValuePair = new KeyValuePair<string, string>(key, keyValuePair[1]);
                              continue;
                         }
                         activeCutsomItem.Add(key, value.Replace("\"", ""));

                         //set key value pair for the created collection keys
                         TreeViewItem newValue = new();
                         newValue.Header = $"{key} : {value}";
                         activeTreeKey.Items.Add(newValue);
                    }
               }

               ExportJsonFile.IsEnabled = true;

               var dictionaryWithMainKey = new Dictionary<string, object>();
               dictionaryWithMainKey.Add("custom_items", custom_items);
               //TreeView.ItemsSource = dictionaryWithMainKey;
               JSONFileData = Newtonsoft.Json.JsonConvert.SerializeObject(dictionaryWithMainKey);
          }

          private void ButtonExportJsonFile_Click(object sender, RoutedEventArgs e)
          {
               this.CreateFile();
          }

          private void CreateFile()
          {
               string path = @"C:\Users\user\Desktop\FAF-3\test.json";

               try
               {
                    // Create the file, or overwrite if the file exists.
                    using (FileStream fs = File.Create(path))
                    {
                         byte[] info = new UTF8Encoding(true).GetBytes(JSONFileData);
                         // Add some information to the file.
                         fs.Write(info, 0, info.Length);
                    }
               }

               catch (Exception ex)
               {
                    Console.WriteLine(ex.ToString());
               }
          }
     }
}
