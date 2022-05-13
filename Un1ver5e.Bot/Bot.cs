using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Text;
using Un1ver5e.BoardGames.Core;
using Un1ver5e.Bot.SlashCommands;
using Un1ver5e.Bot.TextCommands;

namespace Un1ver5e.Bot
{
    public class Bot
    {
        /// <summary>
        /// The main <see cref="DSharpPlus.DiscordClient"/> object.
        /// </summary>
        public DiscordClient? DiscordClient { get; private set; }

        public InteractivityExtension? Interactivity { get; private set; }

        public CommandsNextExtension? CommandsNext { get; private set; }

        public SlashCommandsExtension? SlashCommands { get; private set; }

        public string? Splash { get; private set; }

        /// <summary>
        /// Configures bot.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public void Configure(
            DiscordConfiguration? clientConfigOverride = null, 
            InteractivityConfiguration? interactivityConfigOverride = null,
            CommandsNextConfiguration? commandsNextConfigurationOverride = null,
            SlashCommandsConfiguration? slashCommandsConfigurationOverride = null)
        {
            //Startup
            Logging.ConfigureLogs();

            //DICE
            Dice.CacheCommonDice();

            //SPLASH
            Splash = SplashReader.GetSplash();
            Log.Warning($"Session started >> {Splash}");

            //CLIENT
            DiscordClient = new(clientConfigOverride ?? DefaultClientConfig);

            DiscordClient.GuildDownloadCompleted += async (s, e) => Log.Information($"Guild download complete. Loaded {e.Guilds.Count} guilds.");
            Log.Information("Client set up.");

            //INTERACTIVITY
            Interactivity = DiscordClient.UseInteractivity(interactivityConfigOverride ?? DefaultInteractivityConfig);
            Log.Information("Interactivity set up.");

            //COMMANDSNEXT
            CommandsNext = DiscordClient.UseCommandsNext(commandsNextConfigurationOverride ?? DefaultCommandsNextConfig);
            CommandsNext.RegisterCommands<BasicCommands>();
            CommandsNext.RegisterCommands<BoardGamesCommands>();
            CommandsNext.CommandErrored += async (s, e) =>
            {
                DiscordEmoji respond =
                    e.Exception is DSharpPlus.CommandsNext.Exceptions.CommandNotFoundException ||
                    e.Exception is DSharpPlus.CommandsNext.Exceptions.InvalidOverloadException ?
                    DiscordClient.GetEmoji(":mo_what:") : DiscordClient.GetEmoji(":mo_error:");

                await e.Context.Message.CreateReactionAsync(respond);

                Log.Debug($"Command errored >> {e.Exception.Message}");

                await Task.Run(async () =>
                {
                    var result = await Interactivity.WaitForReactionAsync(r =>
                    r.Message == e.Context.Message &&
                    r.Emoji == respond &&
                    r.User == e.Context.User);

                    if (result.TimedOut == false)
                    {
                        string exceptionMessage = e.Exception.ToString();

                        MemoryStream ms = new(Encoding.Unicode.GetBytes(exceptionMessage));

                        DiscordMessageBuilder dmb = new DiscordMessageBuilder()
                            .WithContent("Текст вашей ошибки:")
                            .WithFile("error.txt", ms);

                        DiscordMessage respond = await e.Context.RespondAsync(dmb);

                        await DiscordClient.ScheduleDestructionAsync(respond);
                    }
                });
            };
            CommandsNext.CommandExecuted += async (s, e) =>
            {
                await e.Context.Message.CreateReactionAsync(DiscordClient.GetEmoji(":mo_ok:"));

                Log.Debug($"Command successfully executed >> {e.Context.Message.Content}");
            };
            Log.Information("CommandsNext set up.");

            //SLASHIES
            SlashCommandsExtension slash = DiscordClient.UseSlashCommands(slashCommandsConfigurationOverride ?? DefaultSlashCommandsConfig);
#if RELEASE
            slash.RegisterCommands<SlashCommandsModule>(956094613536505866);
            slash.RegisterCommands<SlashCommandsModule>(751088089463521322);

            Log.Information("Slashies registered.");
#else
            slash.RegisterCommands<EmptyCommands>(956094613536505866);
            slash.RegisterCommands<EmptyCommands>(751088089463521322);
            Log.Information("Slashies emptied.");
#endif
            Log.Information("Slashies set up.");

            Log.Information("Configursion complete.");
        }

        /// <summary>
        /// Runs bot.
        /// </summary>
        public void Run()
        {
            Task.Run(async () =>
            {
                await DiscordClient!.ConnectAsync(new DiscordActivity(Splash, ActivityType.Watching));

                Log.Information($"{DiscordClient.CurrentUser.Username} is here.");

                await Task.Delay(-1);
            });
        }

        //DEFAULT CONFIGS
        private static DiscordConfiguration DefaultClientConfig => new()
        {
            Intents = DiscordIntents.All,
            MinimumLogLevel = LogLevel.Trace,
            LoggerFactory = new LoggerFactory().AddSerilog(Log.Logger),
            Token = TokenReader.GetToken()
        };

        private static InteractivityConfiguration DefaultInteractivityConfig => new()
        {
            Timeout = TimeSpan.FromMinutes(1),
            AckPaginationButtons = true,
            PollBehaviour = DSharpPlus.Interactivity.Enums.PollBehaviour.KeepEmojis,
            ResponseBehavior = DSharpPlus.Interactivity.Enums.InteractionResponseBehavior.Ack,
        };

        private static CommandsNextConfiguration DefaultCommandsNextConfig => new()
        {
#if DEBUG
            StringPrefixes = new string[] { "mt " }
#else
            StringPrefixes = new string[] { "mo " }
#endif
        };

        private static SlashCommandsConfiguration DefaultSlashCommandsConfig => new()
        {
            
        };
    }
}
