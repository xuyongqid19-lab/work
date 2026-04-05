using System.IO;

var filePath = "ViewModels/HomeViewModel.cs";
var text = File.ReadAllText(filePath);
text = text.Replace("\"口味画像\", \"基于您的口味记录，我们将为您推荐更合适的菜谱\", \"确定\"", "\"Flavor Profile\", \"Based on your taste records, we will recommend more suitable recipes.\", \"OK\"");
File.WriteAllText(filePath, text);

filePath = "ViewModels/NoteViewModel.cs";
text = File.ReadAllText(filePath);
text = text.Replace("\"口味画像\"", "\"Flavor Profile\"");
text = text.Replace("根据您的记录", "Based on your records");
text = text.Replace("偏好标签:", "Preferred categories:");
text = text.Replace("\"确定\"", "\"OK\"");
File.WriteAllText(filePath, text);
