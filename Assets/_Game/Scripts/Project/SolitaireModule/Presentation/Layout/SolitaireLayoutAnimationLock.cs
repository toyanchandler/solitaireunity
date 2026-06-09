using System.Collections;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Presentation
{
    public sealed class SolitaireLayoutAnimationLock
    {
        private readonly MonoBehaviour _host;
        private readonly SolitaireRuntimeContext _context;
        private Coroutine _routine;

        public SolitaireLayoutAnimationLock(MonoBehaviour host, SolitaireRuntimeContext context)
        {
            _host = host;
            _context = context;
        }

        public void LockFor(float duration)
        {
            if (_routine != null)
                _host.StopCoroutine(_routine);

            _context.BeginAnimationLock();
            _routine = _host.StartCoroutine(UnlockAfter(duration));
        }

        public void Cancel()
        {
            if (_routine != null)
            {
                _host.StopCoroutine(_routine);
                _routine = null;
            }

            _context.EndAnimationLock();
        }

        private IEnumerator UnlockAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            _context.EndAnimationLock();
            _routine = null;
        }
    }
}
