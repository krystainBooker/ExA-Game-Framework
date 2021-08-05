﻿using Dialog.Models;
using EventSystem.VisualEditor.Nodes.Actions;
using UnityEngine;

namespace Dialog
{
    public class DialogWriter
    {
        private float _typingAnimationTimer;
        private int _characterIndex;
        private bool _continueClicked;

        private readonly DialogNode _dialogNode;
        private readonly DialogComponents _dialogComponents;
        private readonly float _timePerCharacter;

        private Vector3 _followPlayerOffset;
        private float _displayTimer;

        public DialogWriter(DialogNode dialogNode, DialogComponents dialogComponents)
        {
            _dialogNode = dialogNode;
            _dialogComponents = dialogComponents;
            _timePerCharacter = dialogNode.customTimePerCharacter
                ? dialogNode.timePerCharacter
                : GameManager.Instance.dialogManager.defaultTimePerCharacter;

            //Reset from previous runs
            _characterIndex = 0;
            _continueClicked = false;
        }

        /// <summary>
        /// Sets up the dialog window according to the configuration of the dialogNode
        /// </summary>
        public void Initialize()
        {
            _dialogComponents.dialogGameObject.SetActive(true);

            //Set text
            _dialogComponents.characterNameTMPText.text = _dialogNode.characterName;
            _dialogComponents.dialogTMPText.text = string.Empty;

            //Set size of dialog window
            var width = _dialogNode.dialogWidth != 0
                ? _dialogNode.dialogWidth
                : GameManager.Instance.dialogManager.defaultWidth;
            var height = _dialogNode.dialogHeight != 0
                ? _dialogNode.dialogHeight
                : GameManager.Instance.dialogManager.defaultHeight;
            _dialogComponents.rectTransform.sizeDelta = new Vector2(width, height);

            //Move dialog to position
            var positionX = _dialogNode.customDialogPosition
                ? _dialogNode.dialogPositionX
                : GameManager.Instance.dialogManager.defaultPositionX;
            var positionY = _dialogNode.customDialogPosition
                ? _dialogNode.dialogPositionY
                : GameManager.Instance.dialogManager.defaultPositionY;
            _dialogComponents.rectTransform.anchoredPosition = new Vector2(positionX, positionY);

            //Initial Offset, used when following a character
            if (_dialogNode.followCharacter)
            {
                var characterOriginalPosition =
                    GameManager.Instance.mainCamera.WorldToScreenPoint(_dialogNode.character.transform.position);
                var canvasPosition = _dialogComponents.rectTransform.position;
                _followPlayerOffset = canvasPosition - characterOriginalPosition;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>True on complete</returns>
        public void Update()
        {
            UpdateDialogPosition();
            UpdateText();
        }

        /// <summary>
        /// When the text is fully displayed and the user has clicked continue
        /// or if the dialog has displayed for the required amount of time
        /// then IsNodeFinished will return true
        /// </summary>
        /// <returns>bool</returns>
        public bool IsNodeFinished()
        {
            return (_continueClicked && IsTextFinished()) || (IsTimedDialog() && HasDisplayedForRequiredTime());
        }

        /// <summary>
        /// Returns whether or not the text is fully displayed on screen
        /// </summary>
        /// <returns></returns>
        public bool IsTextFinished()
        {
            return _characterIndex >= _dialogNode.text.Length;
        }

        /// <summary>
        /// Will skip the typing animation and fully display the text on screen instantly
        /// </summary>
        public void SkipTextAnimation()
        {
            //If the dialog box is a timed display node, do not allow the player to skip
            //This is traditionally used for cutscenes, we don't want to mess up timings
            if (_dialogNode.displayForNTime)
                return;

            _characterIndex = _dialogNode.text.Length;
            _dialogComponents.dialogTMPText.text = _dialogNode.text;
        }

        /// <summary>
        /// The textWriter has fully displayed and continue has been clicked, mark as finished 
        /// </summary>
        public void MarkAsFinished()
        {
            if (!IsTextFinished())
                SkipTextAnimation();
            _continueClicked = true;
        }

        /// <summary>
        /// Returns the dialogComponents this dialogWriter instance is using
        /// </summary>
        /// <returns>DialogComponents</returns>
        public DialogComponents GetDialogComponent()
        {
            return _dialogComponents;
        }

        /// <summary>
        /// Returns if the dialog is a timed dialog or a user input dialog
        /// </summary>
        /// <returns></returns>
        public bool IsTimedDialog()
        {
            return _dialogNode.displayForNTime;
        }

        /// <summary>
        /// Returns if the dialog has displayed for the required time
        /// </summary>
        /// <returns>bool</returns>
        public bool HasDisplayedForRequiredTime()
        {
            return _displayTimer >= _dialogNode.displayTime;
        }

        /// <summary>
        /// Allows the dialog box to follow the player around during movement
        /// Offset calculated from initial offset before following player
        /// </summary>
        private void UpdateDialogPosition()
        {
            if (_dialogNode.followCharacter)
            {
                var characterPosition =
                    GameManager.Instance.mainCamera.WorldToScreenPoint(_dialogNode.character.transform.position);
                characterPosition += _followPlayerOffset;
                _dialogComponents.rectTransform.position = characterPosition;
            }
        }

        /// <summary>
        /// Updates the text on the UI with a typing animation
        /// </summary>
        private void UpdateText()
        {
            if (_dialogNode.text.Length <= 0)
                return;

            if (_characterIndex >= _dialogNode.text.Length) //Finished typing
            {
                if (_dialogNode.displayForNTime)
                {
                    _displayTimer += Time.deltaTime;
                }
            }
            else // Typing
            {
                _typingAnimationTimer -= Time.deltaTime;
                while (_typingAnimationTimer <= 0f)
                {
                    //Display next character
                    _typingAnimationTimer += _timePerCharacter;
                    _characterIndex++;

                    //Display all characters, change alpha on unwritten to prevent character format changes
                    var text = _dialogNode.text.Substring(0, _characterIndex);
                    text += $"<alpha=#00>{_dialogNode.text.Substring(_characterIndex)}";
                    _dialogComponents.dialogTMPText.text = text;

                    if (_characterIndex < _dialogNode.text.Length) continue;
                    return;
                }
            }
        }
    }
}