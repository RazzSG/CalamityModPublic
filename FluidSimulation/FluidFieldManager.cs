﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;

namespace CalamityMod.FluidSimulation
{
    public static class FluidFieldManager
    {
        internal static List<FluidField> Fields = new();

        public static FluidField CreateField(int size, float scale, float viscosity, float diffusionFactor, float dissipationFactor)
        {
            var field = new FluidField(size, scale, viscosity, diffusionFactor, dissipationFactor);
            Fields.Add(field);
            return field;
        }

        // Update logic SHOULD NOT be called manually in external parts.
        // A should update and update action field exist to allow for modularity with that.
        // The reason you should not update manually in arbitrary parts of code is because the update loop
        // involves heavy manipulation of render targets, which will fuck up the draw logic of the game
        // if not done at an appropriate point in time.
        public static void Update()
        {
            var old = Main.GameViewMatrix.Zoom;
            Main.GameViewMatrix.Zoom = Vector2.One;
            foreach (FluidField field in Fields)
                field.Update();
            Main.GameViewMatrix.Zoom = old;
        }
    }
}
