using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Commands;
using Prism.Mvvm;
using RiseSim.ViewModels.Controls;
using SimModel.Model;

namespace RiseSim.ViewModels.SubViews;

internal class SkillPickerWindowViewModel : BindableBase, IDisposable
{
    public ObservableCollection<SkillPickerContainerViewModel> ContainerViewModels { get; init; }


    public Action? OnCancel;
    public DelegateCommand OnCanceled => new(Cancel);
    public Action<IReadOnlyList<Skill>>? OnAccept;
    public DelegateCommand OnAccepted => new(Accept);

    public SkillPickerWindowViewModel(IEnumerable<Skill> preSelectedSkills)
    {
        var preSelectedSkillDictionary = preSelectedSkills
            .GroupBy(s => s.Category)
            .ToDictionary(g => g.Key, g => g.ToList());

        ContainerViewModels = new ObservableCollection<SkillPickerContainerViewModel>(
            Masters.Skills
                .GroupBy(s => s.Category)
                .Select(g =>
                {
                    var vm = new SkillPickerContainerViewModel(g.Key, g);
                    if (!preSelectedSkillDictionary.TryGetValue(g.Key, out var value))
                    {
                        return vm;
                    }

                    foreach (var skill in value)
                    {
                        vm.SetPickerSelected(skill);
                    }

                    return vm;
                }));
    }

    private void Cancel()
    {
        OnCancel?.Invoke();
    }

    private void Accept()
    {
        OnAccept?.Invoke(
            ContainerViewModels
                .SelectMany(vm => vm.SelectedSkills())
                .ToList()
        );
    }

    private bool disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }
        if (!disposing)
        {
            return;
        }

        foreach (var skillPickerContainerViewModel in ContainerViewModels)
        {
            skillPickerContainerViewModel.Dispose();
        }

        disposed = true;
    }

    ~SkillPickerWindowViewModel() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}