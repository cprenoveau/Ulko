using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Ulko.Battle
{
    public class Battlefield : MonoBehaviour
    {
        public Vector2 heroStandDirection = new Vector2(-1, 0);
        public Vector2 enemyStandDirection = new Vector2(1, 0);

        public float cameraPanDuration = 2f;
        public Transform cameraStart;
        public Transform cameraEnd;

        public float runDistance = 5f;
        public float runDuration = 1f;

        public Line heroesLine;
        public List<EnemyPosition> enemyPositions = new List<EnemyPosition>();

        public Vector3 HeroPosition(int heroIndex, int heroCount)
        {
            if(heroCount == 1)
            {
                return heroesLine.Lerp(0.5f);
            }
            else if(heroCount == 2)
            {
                return heroesLine.Lerp(heroIndex * 0.6f + 0.2f);
            }
            else
            {
                float step = 1f / (heroCount - 1);
                return heroesLine.Lerp(step * heroIndex);
            }
        }

        public async Task PlayIntroAnim(List<Character> heroes, Camera cam, CancellationToken ct)
        {
            var tasks = new List<Task>
            {
                CameraPan(cam, ct)
            };

            foreach(var hero in heroes)
            {
                tasks.Add(RunIntoBattlefield(hero, ct));
            }

            await Task.WhenAll(tasks).ConfigureAwait(true);
        }

        public async Task RunIntoBattlefield(Character hero, CancellationToken ct)
        {
            Vector3 originalPos = hero.transform.position;
            hero.SetState(Character.AnimState.RunInto);

            Vector3 startPos = originalPos - new Vector3(heroStandDirection.x, 0, heroStandDirection.y) * runDistance;
            hero.transform.position = startPos;

            float elapsed = 0;
            while(!ct.IsCancellationRequested && elapsed < runDuration)
            {
                elapsed += Time.deltaTime;
                hero.transform.position = Vector3.Lerp(startPos, originalPos, elapsed / runDuration);

                await Task.Yield();
            }

            hero.transform.position = originalPos;
            hero.SetState(Character.AnimState.Idle);
        }

        public async Task CameraPan(Camera cam, CancellationToken ct)
        {
            var tween = new HotChocolate.Motion.Tween<Vector3>(cameraPanDuration, cameraStart.position, cameraEnd.position, Vector3.Lerp, HotChocolate.Motion.Easing.Linear);
            tween.OnUpdate += (Vector3 value, float progress) => { UpdateCameraPosition(cam, value, progress); };

            while (!ct.IsCancellationRequested && tween.Play(Time.deltaTime))
            {
                await Task.Yield();
            }
        }

        private void UpdateCameraPosition(Camera cam, Vector3 value, float progress)
        {
            cam.transform.position = new Vector3(value.x, value.y, cam.transform.position.z);
        }
    }
}