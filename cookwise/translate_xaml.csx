using System.IO;

var filePath = "Views/HomePage.xaml";
var text = File.ReadAllText(filePath);
text = text.Replace("精准化烹饪实验室", "Precision Cooking Laboratory");
text = text.Replace("快捷操作", "Quick Actions");
text = text.Replace("精选菜谱", "Featured Recipes");
text = text.Replace("全部菜谱", "All Recipes");
text = text.Replace("人份", " Servings");
File.WriteAllText(filePath, text);

filePath = "Views/SearchPage.xaml";
text = File.ReadAllText(filePath);
text = text.Replace("搜索", "Search");
text = text.Replace("输入食材（逗号分隔）", "Enter ingredients (comma-separated)");
text = text.Replace("搜索菜谱", "Search Recipes");
text = text.Replace("已选食材", "Selected Ingredients");
text = text.Replace("匹配结果", "Match Results");
text = text.Replace("缺少食材:", "Missing Ingredients:");
File.WriteAllText(filePath, text);

filePath = "Views/NotePage.xaml";
text = File.ReadAllText(filePath);
text = text.Replace("烹饪笔记", "Cooking Notes");
text = text.Replace("保存笔记", "Save Note");
text = text.Replace("评分", "Rating");
text = text.Replace("口味偏好记录", "Flavor Profile Record");
text = text.Replace("选择菜谱", "Select Recipe");
File.WriteAllText(filePath, text);
