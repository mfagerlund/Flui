// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using Flui;
using Flui.Binder;
using FluiDemo.Bootstrap;
using FluiDemo.Bootstrap.Binder;
using FluiDemo.Bootstrap.Creator;
using FluiDemo.GameSettings.Binder;
using FluiDemo.GameSettings.Creator;
using FluiDemo.ListUi.Creator;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace FluiDemo.MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        private UIDocument _document;
        private VisualElement _rootVisualElement;

        [SerializeField] private GameSettingsBinder gameSettingsBinder;
        [SerializeField] private GameSettingsCreator gameSettingsCreator;

        [SerializeField] private BootstrapBinderDemo bootstrapBinderDemo;
        [SerializeField] private BootstrapCreatorDemo bootstrapCreatorDemo;

        [SerializeField] private ListUi.Binder.ListUiBinder listUiBinder;
        [SerializeField] private ListUiCreator listUiCreator;

        [SerializeField] private Mixed.Mixed _mixed;

        private readonly FluiBinderRoot<MainMenu, VisualElement> _root = new();
        private List<RandomPosition> _randomPositions;
        private Camera _camera;

        private void OnEnable()
        {
            _document ??= GetComponent<UIDocument>();
            _rootVisualElement = _document.rootVisualElement;
            CommonHelper.FadeIn(this, _rootVisualElement);
        }

        private void Update()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_randomPositions == null)
            {
                _randomPositions =
                    Enumerable
                        .Range(0, 10)
                        .Select(x => new RandomPosition { Id = x, Position = new Vector2(Random.value * Screen.width, Random.value * Screen.height) })
                        .ToList();
            }
            else
            {
                foreach (var randomPosition in _randomPositions)
                {
                    randomPosition.Position += new Vector2(Random.value - 0.5f, Random.value - 0.5f) * 0.1f;
                }
            }

            _root.BindGui(this, _rootVisualElement, x => x
                .Group("FluiBinderDemos", ctx => ctx, g => g
                    .Button("BootstrapDemo", _ => ShowBootstrapBinderDemo())
                    .Button("GameSettingsMenu", _ => ShowGameSettingsBinder())
                    .Button("List", _ => ShowListBinder())
                )
                .Group("FluiCreatorDemos", ctx => ctx, g => g
                    .Button("BootstrapDemo", _ => ShowBootstrapCreatorDemo())
                    .Button("GameSettingsMenu", _ => ShowGameSettingsCreator())
                    .Button("List", _ => ShowListCreator())
                )
                .Button("Mixed", _ => ShowMixed())
                .Label("Time", _ => $"Time: {DateTime.Now:HH:mm:ss}")
                .Label("MouseFollowerBound", l => "Bound Follower", null, null, l => l
                    .SetScreenXy(Input.mousePosition, AlignX.Mid, AlignY.Top)
                    .SetPickingMode(PickingMode.Ignore)
                )
                .Create("CreatedStuff", x => x
                    , c => c
                        .SetPosition(Position.Absolute)
                        .ForEach(fe => fe._randomPositions, "", rpo => rpo
                            .Label("label", rp => $"[{rp.Id}]: ({rp.Position.x:0}, {rp.Position.y:0})", "", l => l
                                .Action(x => x.Element.style.unityFontStyleAndWeight = FontStyle.Bold)
                                .SetScreenXy(l.Context.Position, AlignX.Mid, AlignY.Mid)
                                .SetPickingMode(PickingMode.Ignore)
                            )
                        )
                        .Label("MouseFollowerCreated", _ => "Created Follower", "", l => l
                            .SetScreenXy(Input.mousePosition, AlignX.Mid, AlignY.Bottom)
                            .SetPickingMode(PickingMode.Ignore)
                        )
                )
            );
        }

        private void ShowBootstrapBinderDemo()
        {
            Hide();
            bootstrapBinderDemo.Show(Show);
        }

        private void ShowBootstrapCreatorDemo()
        {
            Hide();
            bootstrapCreatorDemo.Show(Show);
        }

        private void ShowGameSettingsBinder()
        {
            Hide();
            gameSettingsBinder.Show(Show);
        }

        private void ShowGameSettingsCreator()
        {
            Hide();
            gameSettingsCreator.Show(Show);
        }

        private void ShowListBinder()
        {
            Hide();
            listUiBinder.Show(Show);
        }

        private void ShowListCreator()
        {
            Hide();
            listUiCreator.Show(Show);
        }

        private void ShowMixed()
        {
            Hide();
            _mixed.Show(Show);
        }

        private void Show()
        {
            gameObject.SetActive(true);
        }

        private void Hide()
        {
            CommonHelper.FadeOut(
                this,
                _rootVisualElement,
                () => gameObject.SetActive(false));
        }

        private class RandomPosition
        {
            public int Id { get; set; }
            public Vector2 Position { get; set; }
        }
    }
}