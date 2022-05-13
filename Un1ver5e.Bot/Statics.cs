﻿using DSharpPlus.Entities;
using System.Text.Json;

namespace Un1ver5e.Bot
{
    /// <summary>
    /// Contains some useful features.
    /// </summary>
    public static class Statics
    {
        /// <summary>
        /// The ID of "MO Hub" guild.
        /// </summary>
        public static readonly ulong HubGuildID = 956094613536505866;

        /// <summary>
        /// The absolute path to the app folder. Does not contain a '/' symbol at the end.
        /// </summary>
        public static readonly string AppPath = Environment.CurrentDirectory;

        /// <summary>
        /// The absolute path to the Data folder. Does not contain a '/' symbol at the end.
        /// </summary>
        public static readonly string DataFolderPath = AppPath + "/Data";

        public static TimeSpan MessageDestructionTime { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// The default-colored embed, using this is recommended.
        /// </summary>
        public static DiscordEmbed EmbedTemplate { get; set; } = new DiscordEmbedBuilder().WithColor(0x3aebca).Build();

        /// <summary>
        /// The default <see cref="System.Text.Json"/> configuration, is advised to use everywhere in code
        /// </summary>
        public static JsonSerializerOptions JsonSerializerOptions { get; set; } = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
        };
    }
}
