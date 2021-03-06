using System.Collections;
using EventSystem.VisualEditor.Graphs;
using UnityEngine;

namespace EventSystem.Triggers
{
    /// <summary>
    /// The triggerEventSequence will begin when this component is initialized.
    /// This should be added to a gameObject to start at the beginning a scene
    /// or when a gameObject in instantiated 
    /// </summary>
    public class OnStartTrigger : MonoBehaviour
    {
        [Tooltip("Event Sequence to be triggered")]
        public EventSequenceGraph triggerEventSequence;

        private EventSequenceParser _triggerEventSequenceParser;

        private void Start()
        {
            StartCoroutine(BeginTriggerEvent());
        }

        /// <summary>
        /// Triggers the event sequence as part of a coroutine
        /// </summary>
        /// <returns></returns>
        private IEnumerator BeginTriggerEvent()
        {
            if (triggerEventSequence == null)
                yield break;

            //Add trigger event timeline parser
            if (_triggerEventSequenceParser == null)
            {
                _triggerEventSequenceParser = gameObject.AddComponent<EventSequenceParser>();
            }

            //Start trigger event sequence
            StartCoroutine(_triggerEventSequenceParser.StartEventSequence(triggerEventSequence));
            yield return new WaitUntil(_triggerEventSequenceParser.IsEventSequenceFinished);
        }
    }
}