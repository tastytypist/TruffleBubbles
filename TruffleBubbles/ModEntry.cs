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
    // Truffle tile cache.
    private readonly HashSet<Vector2> _truffleTiles = new();

    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        // Hook game events.
        helper.Events.Display.RenderedWorld += OnRenderedWorld;
        helper.Events.Player.Warped += OnWarped;
        helper.Events.World.ObjectListChanged += OnObjectListChanged;
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
            // Get all truffles in the farm.
            foreach (Vector2 truffleTile in _truffleTiles)
            {
                ParsedItemData truffle = ItemRegistry.GetDataOrErrorItem("(O)430"); // Truffle item id

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

    /// <summary>Raised after a player warps to a new location.
    /// NOTE: this event is currently only raised for the current player.</summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnWarped(object? sender, WarpedEventArgs e)
    {
        // Cache the truffle tiles if the local farmhand is in their farm.
        if (e is { NewLocation: Farm, IsLocalPlayer: true })
        {
            foreach (Object existingObject in e.NewLocation.Objects.Values)
            {
                if (existingObject.QualifiedItemId == "(O)430")
                {
                    Vector2 objectTile = existingObject.TileLocation;
                    if (_truffleTiles.Add(objectTile))
                    {
                        Monitor.Log($"[OnWarped] Truffle tile is added - {objectTile}", LogLevel.Debug);
                    }
                }
            }
        }
    }

    /// <summary>Raised after objects are added or removed in a location.</summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e)
    {
        // Ensure the change happens on the farmhand's farm.
        if (e.Location is Farm)
        {
            // Remove the truffle tile from the cache after being picked up.
            foreach (KeyValuePair<Vector2, Object> removedObject in e.Removed)
            {
                Vector2 objectTile = removedObject.Key;
                if (removedObject.Value.QualifiedItemId == "(O)430" && _truffleTiles.Remove(objectTile))
                {
                    Monitor.Log($"[OnObjectListChanged] Truffle tile is removed - {objectTile}", LogLevel.Debug);
                }
            }
            // Add the truffle tile to the cache upon finding it.
            foreach (KeyValuePair<Vector2, Object> addedObject in e.Added)
            {
                Vector2 objectTile = addedObject.Key;
                if (addedObject.Value.QualifiedItemId == "(O)430" && _truffleTiles.Add(objectTile))
                {
                    Monitor.Log($"[OnObjectListChanged] Truffle tile is added - {objectTile}", LogLevel.Debug);
                }
            }
        }
    }
}
