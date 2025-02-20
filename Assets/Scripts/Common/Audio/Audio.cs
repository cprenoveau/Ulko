
namespace Ulko
{
    public interface IAudioPlayer
    {
        void Play(AudioDefinition audioDef, float volume = 1f, float fadeInDuration = 2f);
        void PlaySolo(AudioDefinition audioDef, float volume = 1f, float fadeInDuration = 2f);
        void PlayUISound(Audio.UISoundId id);

        void StopAll(AudioDefinition audioDef, float fadeOutDuration = 2F);
        void StopAll(AudioType type, float fadeOutDuration = 2F);
    }

    public class NullAudioPlayer : IAudioPlayer
    {
        public void Play(AudioDefinition audioDef, float volume = 1, float fadeInDuration = 2) { }
        public void PlaySolo(AudioDefinition audioDef, float volume = 1, float fadeInDuration = 2) { }
        public void PlayUISound(Audio.UISoundId id) { }

        public void StopAll(AudioDefinition audioDef, float fadeOutDuration = 2) { }
        public void StopAll(AudioType type, float fadeOutDuration = 2) { }
    }

    public static class Audio
    {
        public enum UISoundId
        {
            MenuBlip = 0,
            MenuOk = 1,
            MenuCancel = 2,
            NewGame = 3,
            LoadGame = 4,
            SaveGame = 5,
            QuitGame = 6,
            Equip = 7,
            ItemUseMenu = 8,
            ItemGain = 9,
            BuyItem = 10,
            Swish = 11,
            DialogChar = 100,
            DialogEnd = 101
        }

        public static IAudioPlayer Player { get; private set; } = new NullAudioPlayer();

        public static void Init(IAudioPlayer player)
        {
            if (player != null)
            {
                Player = player;
            }
        }
    }
}
