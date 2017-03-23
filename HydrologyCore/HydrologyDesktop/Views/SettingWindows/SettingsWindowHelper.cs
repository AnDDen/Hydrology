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

            if (nodeType == typeof(AlgorithmNode))
                window = new AlgorithmSettingsWindow(algorithmType, container.NodeContainer);
            if (nodeType == typeof(LoopNode))
                window = new LoopNodeSettingsWindow(container.NodeContainer);
            if (nodeType == typeof(InitNode))
                window = new InitNodeSettingsWindow(container.NodeContainer);
            // todo

            if (window != null)
            {
                window.Owner = owner;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            return window;
        }

        // todo
        public static SettingsWindow CreateSettingWindowForNode(AbstractNode node, Window owner)
        {
            SettingsWindow window = null;

            if (node is AlgorithmNode)
                window = new AlgorithmSettingsWindow(node);
            if (node is LoopNode)
                window = new LoopNodeSettingsWindow(node);
            if (node is InitNode)
                window = new InitNodeSettingsWindow(node);

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
