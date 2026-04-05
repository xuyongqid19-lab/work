using System.IO;
using System.Text.RegularExpressions;

var filePath = "Services/RecipeService.cs";
var text = File.ReadAllText(filePath);

text = text.Replace("Description = \"经典家常菜，酸甜可口，简单易学\",", "Description = \"经典家常菜，酸甜可口，简单易学 (Classic home-style dish, sweet & sour, easy to make)\",");
text = text.Replace("Description = \"Melt in your mouth braised pork belly\",", "Description = \"肥而不腻，入口即化，下饭神器 (Melt in your mouth braised pork belly, perfect with rice)\",");
text = text.Replace("Description = \"麻辣鲜香，鸡肉嫩滑，花生脆爽\",", "Description = \"麻辣鲜香，鸡肉嫩滑，花生脆爽 (Spicy and fresh, tender chicken, crispy peanuts)\",");
text = text.Replace("Description = \"麻辣鲜香，豆腐嫩滑，川菜经典\",", "Description = \"麻辣鲜香，豆腐嫩滑，川菜经典 (Spicy, numbing, tender tofu, a Sichuan classic)\",");
text = text.Replace("Description = \"酸甜可口，外酥里嫩，老少皆宜\",", "Description = \"酸甜可口，外酥里嫩，老少皆宜 (Sweet and sour, crispy outside and tender inside)\",");
text = text.Replace("Description = \"清淡鲜美，简单快手，营养丰富\",", "Description = \"清淡鲜美，简单快手，营养丰富 (Light and delicious, quick to make, nutritious)\",");
text = text.Replace("Description = \"香甜软嫩，老少皆爱的懒人菜\",", "Description = \"香甜软嫩，老少皆爱的懒人菜 (Sweet and tender, a lazy dish loved by all ages)\",");
text = text.Replace("Description = \"清脆爽口，蒜香浓郁，健康素菜\",", "Description = \"清脆爽口，蒜香浓郁，健康素菜 (Crispy and refreshing, rich garlic flavor, healthy veg)\",");
text = text.Replace("Description = \"清香爽口，色彩诱人，家常快手菜\",", "Description = \"清香爽口，色彩诱人，家常快手菜 (Refreshing, colorful, quick home-style dish)\",");
text = text.Replace("Description = \"爽脆可口，酸辣开胃，简单实惠\",", "Description = \"爽脆可口，酸辣开胃，简单实惠 (Crispy, sour and spicy appetizer, simple and affordable)\",");
text = text.Replace("Description = \"软糯入味，酱香浓郁，下饭神器\",", "Description = \"软糯入味，酱香浓郁，下饭神器 (Soft and flavorful, rich sauce, perfect with rice)\",");
text = text.Replace("Description = \"咸甜酸辣兼备，木耳脆爽，川菜经典\",", "Description = \"咸甜酸辣兼备，木耳脆爽，川菜经典 (Salty, sweet, sour & spicy, a Sichuan classic)\",");

File.WriteAllText(filePath, text);
