# CookWise 🍳

精准化烹饪实验室 - 一款基于 .NET MAUI 的跨平台菜谱应用。

## ✨ 功能特性

- **菜谱浏览** - 浏览精选菜谱，查看详细步骤
- **智能搜索** - 根据现有食材搜索匹配菜谱
- **份量调整** - 动态调整菜谱份量，自动计算食材用量
- **烹饪计时器** - 内置计时器，精确控制烹饪时间
- **口味画像** - 记录烹饪笔记，构建个人口味偏好

## 📱 截图

> 待添加

## 🛠️ 技术栈

- **框架**: .NET MAUI (.NET10)
- **架构**: MVVM (CommunityToolkit.Mvvm)
- **平台支持**: Windows, Android, iOS, macOS

## 📁 项目结构

```
cookwise/
├── Models/                 # 数据模型
│   ├── Ingredient.cs       # 食材
│   ├── CookingStep.cs     # 烹饪步骤
│   ├── Recipe.cs          # 菜谱
│   ├── UserNote.cs        # 用户笔记
│   ├── FlavorProfile.cs   # 口味画像
│   └── SearchResult.cs    # 搜索结果
├── ViewModels/            # 视图模型
│   ├── HomeViewModel.cs
│   ├── SearchViewModel.cs
│   ├── RecipeDetailViewModel.cs
│   └── NoteViewModel.cs
├── Views/                 # 视图页面
│   ├── HomePage.xaml
│   ├── SearchPage.xaml
│   ├── RecipeDetailPage.xaml
│   ├── LoginPage.xaml
│   └── NotePage.xaml
├── Services/              # 服务层
│   └── RecipeService.cs   # 菜谱数据服务
├── Resources/              # 资源文件
│   ├── Styles/            # 样式定义
│   │   ├── Colors.xaml    # 颜色资源
│   │   └── Styles.xaml    # 样式资源
│   ├── Images/            # 图片资源
│   └── Fonts/             # 字体资源
└── Platforms/             # 平台特定代码
    ├── Windows/
    ├── Android/
    ├── iOS/
    └── MacCatalyst/
```

## 🚀 快速开始

### 前置要求

- [.NET10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 或 VS Code + C# Dev Kit
- 目标平台对应的开发环境

### 运行项目

```bash
# 克隆仓库
git clone <repository-url>
cd cookwise

# 还原依赖
dotnet restore

# 运行 (Windows)
dotnet run -f net10.0-windows10.0.19041.0

# 或在 Visual Studio 中打开 cookwise.slnx 并运行
```

## 🎨 自定义样式

应用使用集中式颜色和样式管理：

### 品牌色

| 颜色键 | 值 | 用途 |
|--------|-----|------|
| `BrandPrimary` | `#FF6B35` | 主色调 |
| `BrandPrimaryDark` | `#E65100` | 深色变体 |
| `BrandPrimaryLight` | `#FFF3E0` | 浅色变体 |

### 预定义样式

- `CardFrame` - 卡片容器
- `PrimaryButton` - 主按钮
- `SuccessButton` / `DangerButton` - 语义按钮
- `TitleLabel` / `SubtitleLabel` - 文本样式

## 📡 API 集成

应用默认连接到 `https://cook.yunyoujun.cn/api` 获取菜谱数据。

API 失败时会自动回退到内置的示例数据。

## 🤝 贡献指南

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/amazing-feature`)
3. 提交更改 (`git commit -m 'Add amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 提交 Pull Request

## 🙏 致谢

- [.NET MAUI](https://docs.microsoft.com/dotnet/maui/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)