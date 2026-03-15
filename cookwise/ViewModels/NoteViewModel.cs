using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using cookwise.Models;
using cookwise.Services;

namespace cookwise.ViewModels;
    public partial class NoteViewModel : ObservableObject
{
    [ObservableProperty]
    private UserNote _newNote = new();

    [ObservableProperty]
    private ObservableCollection<UserNote> _userNotes = new();

    public NoteViewModel()
    {
        LoadNotes();
    }

    private async void LoadNotes()
    {
        var service = RecipeService.Instance;
        var notes = await service.GetUserNotesAsync();
        UserNotes = new ObservableCollection<UserNote>(notes);
    }

    [RelayCommand]
    private async Task SaveNote()
    {
        if (string.IsNullOrWhiteSpace(NewNote.RecipeName))
            return;

        var service = RecipeService.Instance;
        NewNote.Id = Guid.NewGuid().ToString();
        NewNote.CreatedAt = DateTime.Now;
        await service.SaveUserNoteAsync(NewNote);
        
        LoadNotes();
        
        NewNote = new UserNote();
    }

    [RelayCommand]
    private async Task ViewProfile()
    {
        var service = RecipeService.Instance;
        var profile = await service.GetFlavorProfileAsync();
        
        var preferences = string.Join(", ", profile.TastePreferences.Keys);
        await Shell.Current.DisplayAlert("口味画像", 
            $"您的口味偏好: {preferences}\n\n基于您的记录，系统将为您推荐更合适的菜谱。", "确定");
    }
}
