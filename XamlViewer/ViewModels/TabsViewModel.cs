﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using XamlService.Commands;
using XamlService.Events;
using XamlUtil.Common;
using XamlViewer.Models;

namespace XamlViewer.ViewModels
{
    public class TabsViewModel : BindableBase
    {
        private XamlConfig _xamlConfig = null;
        private IEventAggregator _eventAggregator = null;

        public ObservableCollection<TabViewModel> Tabs { get; set; }

        public DelegateCommand NewCommand { get; private set; } 

        public TabsViewModel(IContainerExtension container, IEventAggregator eventAggregator, IApplicationCommands appCommands)
        {
            _xamlConfig = container.Resolve<XamlConfig>();

            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<CloseTabEvent>().Subscribe(Close);


            NewCommand = new DelegateCommand(New);
            appCommands.NewCommand.RegisterCommand(NewCommand); 

            Tabs = new ObservableCollection<TabViewModel>(_xamlConfig.Files.Select(f => new TabViewModel(f)));
            if (Tabs.Count == 0)
                Tabs.Add(new TabViewModel(Common.GetCopyName("NewFile", " ", n => false) + ".xaml") { Status = TabStatus.NoSave });

            Tabs[0].IsSelected = true;
        }

        private void New()
        {
            Tabs.Add(new TabViewModel(Common.GetCopyName("NewFile", " ", n => Tabs.Any(tab => Path.GetFileNameWithoutExtension(tab.Title).ToLower() == n.ToLower())) + ".xaml")
            {
                IsSelected = true,
                Status = TabStatus.NoSave
            });
        } 

        private void Close(string fileName)
        {
            var tab = Tabs.FirstOrDefault(t => t.FileName == fileName);
            if(tab != null)
            {
                Tabs.Remove(tab);

                if (Tabs.Count == 0)
                    New();

                if(tab.IsSelected)
                    Tabs[0].IsSelected = true;
            }

            _xamlConfig.Files.RemoveAll(f => f == tab.FileName);
        }
    }
}
