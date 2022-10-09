/*    RiseSim : MHRise skill simurator for Windows
 *    Copyright (C) 2022  EXXXI
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
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