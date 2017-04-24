﻿using HydrologyCore.Experiment;
using HydrologyCore.Experiment.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HydrologyDesktop.Views.SettingWindows
{
    /// <summary>
    /// Логика взаимодействия для BlockSettingsWindow.xaml
    /// </summary>
    public partial class BlockSettingsWindow : SettingsWindow
    {
        public BlockSettingsWindow(HydrologyCore.Experiment.Block container, bool isloop) 
            : this((isloop ? new LoopBlock("", container) : new HydrologyCore.Experiment.Block("", container))) { }

        public BlockSettingsWindow(IRunable node)
        {
            Node = node;

            InitializeComponent();
        }
    }
}
