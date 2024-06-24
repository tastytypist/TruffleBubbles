/*
 * TruffleBubbles - Display bubble indicators for locating truffles.
 * Copyright (C) 2024-present tastytypist
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using Object = StardewValley.Object;

namespace TruffleBubbles;

/// <summary>The mod entry point.</summary>
internal sealed class ModEntry : Mod
{
    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        // Hook game events.
        helper.Events.Display.RenderedWorld += OnRenderedWorld;
    }

    /// <summary>Raised after the game world is drawn to the sprite patch, before it's rendered to the screen.
    /// Content drawn to the sprite batch at this point will be drawn over the world, but under any active menu,
    /// HUD elements, or cursor.</summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
    {
        // Check whether
        // 1. a game has been loaded and the world is initialised, and
        // 2. the player is currently in their farm.
        if (Context.IsWorldReady && Game1.player.currentLocation is Farm)
        {
            // Find all truffles in the farm.
            foreach (Object drawnObject in Game1.player.currentLocation.Objects.Values)
            {
                if (drawnObject.QualifiedItemId == "(O)430") // Truffle item id.
                {
                    Vector2 truffleTile = drawnObject.TileLocation;
                    ParsedItemData truffle = ItemRegistry.GetDataOrErrorItem("(O)430");
    
                    // Draw the bubble container.
                    // Refer to `StardewValley.Object.draw()`.
                    Texture2D bubbleTexture = Game1.mouseCursors;
                    float bubbleOffset =
                        4f
                        * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
                    Vector2 bubbleLocation = new Vector2(
                        truffleTile.X * 64 - 8,
                        truffleTile.Y * 64 - 96 - 16 + bubbleOffset
                    );
                    Vector2 bubbleLocalLocation = Game1.GlobalToLocal(Game1.viewport, bubbleLocation);
                    Rectangle bubbleTextureLocation = new Rectangle(141, 465, 20, 24);
                    Color bubbleColourMask = Color.White * 0.75f;
                    float bubbleRotation = 0f;
                    Vector2 bubbleOrigin = Vector2.Zero;
                    float bubbleScaling = 4f;
                    SpriteEffects bubbleEffects = SpriteEffects.None;
                    float bubbleLayerDepth = (truffleTile.Y + 1) * 64f / 10000f + truffleTile.X / 50000f + 1E-06f;
                    e.SpriteBatch.Draw(
                        bubbleTexture,
                        bubbleLocalLocation,
                        bubbleTextureLocation,
                        bubbleColourMask,
                        bubbleRotation,
                        bubbleOrigin,
                        bubbleScaling,
                        bubbleEffects,
                        bubbleLayerDepth
                    );
    
                    // Draw the truffle inside the bubble.
                    // Refer to `StardewValley.Object.draw()`.
                    Texture2D truffleTexture = truffle.GetTexture();
                    float truffleOffset =
                        4f
                        * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
                    Vector2 truffleBubbleLocation = new Vector2(
                        truffleTile.X * 64 + 32,
                        truffleTile.Y * 64 - 64 - 8 + truffleOffset
                    );
                    Vector2 truffleBubbleLocalLocation = Game1.GlobalToLocal(Game1.viewport, truffleBubbleLocation);
                    Rectangle truffleTextureLocation = truffle.GetSourceRect();
                    Color truffleColourMask = Color.White * 0.75f;
                    float truffleRotation = 0f;
                    Vector2 truffleOrigin = new Vector2(8f, 8f);
                    float truffleScaling = 4f;
                    SpriteEffects truffleEffects = SpriteEffects.None;
                    float truffleLayerDepth = (truffleTile.Y + 1) * 64f / 10000f + truffleTile.X / 50000f + 1E-05f;
                    e.SpriteBatch.Draw(
                        truffleTexture,
                        truffleBubbleLocalLocation,
                        truffleTextureLocation,
                        truffleColourMask,
                        truffleRotation,
                        truffleOrigin,
                        truffleScaling,
                        truffleEffects,
                        truffleLayerDepth
                    );
                }
            }
        }
    }
}
