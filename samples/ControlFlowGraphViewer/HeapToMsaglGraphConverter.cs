﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Heap;
using Microsoft.Msagl.Drawing;

namespace ControlFlowGraphViewer
{
    public class HeapToMsaglGraphConverter
    {
        public Graph Convert(IHeapModel heap, int version)
        {
            var graph = new Graph();

            foreach (var location in heap.GetLocations(version))
            {
                var node = graph.AddNode(location.Id.ToString());

                node.LabelText = location.ToString();

                foreach (var reference in heap.GetReferences(location))
                {
                    graph.AddEdge(location.Id.ToString(), reference.Field.ToString(), reference.LocationId.ToString());
                }

                foreach (var value in heap.GetValues(location))
                {
                    node.LabelText += $"\n{value.Field} = {value.Value}";
                }
            }

            return graph;
        }
    }
}
