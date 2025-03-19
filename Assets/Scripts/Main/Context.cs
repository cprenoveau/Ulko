using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

namespace Ulko
{
    public abstract class Context : MonoBehaviour
    {
        public abstract ContextType ContextType { get; }
        public abstract UIRoot UIRoot { get; }
        public abstract Camera Camera { get; }
        public abstract Camera UICamera { get; }

        public abstract void Init(object data);

        public abstract Task Begin(CancellationToken ct);
        public abstract Task End(CancellationToken ct);

        public abstract void Pause();
        public abstract void Resume();

        public abstract void Move(Vector2 direction, float deltaTime);
        public abstract void Interact();
        public abstract void Cancel();
        public abstract bool Back();
        public abstract void OpenMenu();
    }

    public abstract class Context<T> : Context where T : class
    {
        public UIRoot uiRoot;
        public override UIRoot UIRoot => uiRoot;

        public bool IsActive => gameObject.activeInHierarchy;
        public bool IsPaused { get; private set; }

        public T Data { get; private set; }

        public override void Init(object data)
        {
            Init(data as T);
        }

        public void Init(T data)
        {
            Data = data;
            uiRoot.Init();
        }

        public override async Task Begin(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await _Begin(ct);

            Resume();
        }

        public override async Task End(CancellationToken ct)
        {
            await _End(ct);
            gameObject.SetActive(false);
        }

        public override void Pause()
        {
            if (!IsPaused)
            {
                uiRoot.BlockInput(releaseAllOtherBlockers:true);
            }

            IsPaused = true;
        }

        public override void Resume()
        {
            if (IsPaused)
            {
                uiRoot.UnblockInput(releaseAllBlockers:true);
            }

            IsPaused = false;
        }

        public override void Move(Vector2 direction, float deltaTime)
        {
            if (!IsPaused) _Move(direction, deltaTime);
        }

        public override void Interact()
        {
            if (!IsPaused) _Interact();
        }

        public override void Cancel()
        {
            if (!IsPaused) _Cancel();
        }

        public override bool Back()
        {
            if (!IsPaused) return _Back();
            return false;
        }

        public override void OpenMenu()
        {
            if (!IsPaused) _OpenMenu();
        }

        private void OnDestroy()
        {
            _Dispose();
        }

        protected abstract Task _Begin(CancellationToken ct);
        protected abstract Task _End(CancellationToken ct);

        protected abstract void _Move(Vector2 direction, float deltaTime);
        protected abstract void _Interact();
        protected abstract void _Cancel();
        protected abstract bool _Back();
        protected abstract void _OpenMenu();

        protected abstract void _Dispose();
    }
}
