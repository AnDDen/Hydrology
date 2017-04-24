using HydrologyCore.Experiment;
using HydrologyCore.Experiment.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HydrologyDesktop.Views.SettingWindows
{
    public class SettingsWindowHelper
    {
        // todo
        public static SettingsWindow CreateSettingWindow(Type nodeType, Type algorithmType, NodeContainerGraph container, Window owner)
        {
            SettingsWindow window = null;

            if (nodeType == typeof(LoopBlock))
                window = new BlockSettingsWindow(container.Block, true);
            if (nodeType == typeof(Block))
                window = new BlockSettingsWindow(container.Block, false);
            if (nodeType == typeof(AlgorithmNode))
                window = new AlgorithmSettingsWindow(algorithmType, container.Block);
            if (nodeType == typeof(InitNode))
                window = new InitNodeSettingsWindow(container.Block);
            if (nodeType == typeof(InPortNode))
                window = new PortNodeSettingsWindow(container.Block, true);
            if (nodeType == typeof(OutPortNode))
                window = new PortNodeSettingsWindow(container.Block, false);

            // todo

            if (window != null)
            {
                window.Owner = owner;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            return window;
        }

        // todo
        public static SettingsWindow CreateSettingWindowForNode(IRunable node, Window owner)
        {
            SettingsWindow window = null;

            if (node is Block)
                window = new BlockSettingsWindow(node);
            if (node is AlgorithmNode)
                window = new AlgorithmSettingsWindow(node);
            if (node is InitNode)
                window = new InitNodeSettingsWindow(node);
            if (node is PortNode)
            {
                if (node is LoopPortNode)
                    window = new LoopPortNodeSettingsWindow(node);
                else
                    window = new PortNodeSettingsWindow(node);
            }

            // todo

            if (window != null)
            {
                window.Owner = owner;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            return window;
        }
    }
}
