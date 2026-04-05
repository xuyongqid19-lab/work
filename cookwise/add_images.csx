using System.IO;
using System.Text.RegularExpressions;

var filePath = "Services/RecipeService.cs";
var text = File.ReadAllText(filePath);

text = text.Replace("Name = \"番茄炒蛋\",", "Name = \"番茄炒蛋\",\n                ImageUrl = \"https://images.unsplash.com/photo-1598514982205-f36b96d1e8dd?auto=format&fit=crop&q=80&w=800\",\n                CookingTime = \"15 mins\",\n                Calories = 250,");
text = text.Replace("Name = \"宫保鸡丁\",", "Name = \"宫保鸡丁\",\n                ImageUrl = \"https://images.unsplash.com/photo-1525755662778-989d0524087e?auto=format&fit=crop&q=80&w=800\",\n                CookingTime = \"25 mins\",\n                Calories = 400,");
text = text.Replace("Name = \"麻婆豆腐\",", "Name = \"麻婆豆腐\",\n                ImageUrl = \"https://images.unsplash.com/photo-1627844642677-8b3d6cb46f7e?auto=format&fit=crop&q=80&w=800\",\n                CookingTime = \"20 mins\",\n                Calories = 320,");
text = text.Replace("Name = \"糖醋排骨\",", "Name = \"糖醋排骨\",\n                ImageUrl = \"https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&q=80&w=800\",\n                CookingTime = \"40 mins\",\n                Calories = 500,");
text = text.Replace("Name = \"西红柿鸡蛋汤\",", "Name = \"西红柿鸡蛋汤\",\n                ImageUrl = \"https://images.unsplash.com/photo-1547592166-23ac45744acd?auto=format&fit=crop&q=80&w=800\",\n                CookingTime = \"10 mins\",\n                Calories = 120,");
text = text.Replace("Name = \"可乐鸡翅\",", "Name = \"可乐鸡翅\",\n                ImageUrl = \"https://images.unsplash.com/photo-1567620832903-9fc6debc209f?auto=format&fit=crop&q=80&w=800\",\n                CookingTime = \"30 mins\",\n                Calories = 450,");
text = text.Replace("Name = \"蒜蓉西兰花\",", "Name = \"蒜蓉西兰花\",\n                ImageUrl = \"https://images.unsplash.com/photo-1505253758473-96b7015fcd40?auto=format&fit=crop&q=80&w=800\",\n                CookingTime = \"10 mins\",\n                Calories = 80,");
text = text.Replace("Name = \"青椒肉丝\",", "Name = \"青椒肉丝\",\n                ImageUrl = \"https://images.unsplash.com/photo-1640715367503-6232d308dbbb?auto=format&fit=crop&q=80&w=800\",\n                CookingTime = \"15 mins\",\n                Calories = 350,");
text = text.Replace("Name = \"酸辣土豆丝\",", "Name = \"酸辣土豆丝\",\n                ImageUrl = \"https://images.unsplash.com/photo-1615486171448-4fd376174a7b?auto=format&fit=crop&q=80&w=800\",\n                CookingTime = \"15 mins\",\n                Calories = 180,");
text = text.Replace("Name = \"红烧茄子\",", "Name = \"红烧茄子\",\n                ImageUrl = \"https://images.unsplash.com/photo-1582283925769-6503c4f74d9e?auto=format&fit=crop&q=80&w=800\",\n                CookingTime = \"25 mins\",\n                Calories = 280,");
text = text.Replace("Name = \"鱼香肉丝\",", "Name = \"鱼香肉丝\",\n                ImageUrl = \"https://images.unsplash.com/photo-1635334795738-f949c258ce88?auto=format&fit=crop&q=80&w=800\",\n                CookingTime = \"20 mins\",\n                Calories = 380,");

File.WriteAllText(filePath, text);
