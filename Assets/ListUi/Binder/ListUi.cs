// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using Flui;
using Flui.Binder;
using FluiDemo.Bootstrap;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace FluiDemo.ListUi.Binder
{
    public class ListUi : MonoBehaviour
    {
        private UIDocument _document;
        [SerializeField] private UIDocument _listRow;
        [SerializeField] private bool _rebuild;
        [SerializeField] private bool _pause;
        [SerializeField] public string _hierarchy;

        private FluiBinderRoot<ListUi, VisualElement> _root = new();
        private VisualElement _rootVisualElement;

        private Action _onHide;

        public void Show(Action onHide)
        {
            _onHide = onHide;
            gameObject.SetActive(true);
        }

        public void OnEnable()
        {
            Connect();
            CommonHelper.FadeIn(this, _rootVisualElement);
        }

        private void Hide()
        {
            // gameObject.SetActive(false);
            CommonHelper.FadeOut(
                this,
                _rootVisualElement,
                () => gameObject.SetActive(false));
            _onHide();
        }

        // private void OnValidate()
        // {
        //     if (Application.isPlaying || !gameObject.activeSelf) return;
        //     Bind();
        // }

        private void Update()
        {
            Bind();
        }

        private TemplateContainer _template;

        private void Bind()
        {
            Connect();
            if (_rebuild)
            {
                _root = new FluiBinderRoot<ListUi, VisualElement>();
                _rebuild = false;
            }

            if (_template == null && _rootVisualElement != null)
            {
               _template = _rootVisualElement.Q("ExternalTemplateList").Q("Content").Q<TemplateContainer>("ListRowUxml");
               _template.parent.Remove(_template);
            }

            _root.BindGui(this, _rootVisualElement, r => r
                .Button("Close", _ => Hide())
                .Group("ExternalTemplateList", x => x, b => b
                    .Button("Add", x => AddRandomEmployee())
                    .ForEach("Content", x => x._employees, () => _template.templateSource.CloneTree(), row => row
                        .Label("Name", x => x.Name)
                        .Label("Title", x => x.Title)
                        .Label("Salary", x => $"{x.Salary:0}")
                        .Button("Delete", x => { DeleteEmployee(row, x); })
                    ))
                .Group("Footer", x => x, g => g
                    .Label("Salary", ctx => $"{GetSalarySum():0}")
                )
            );

            _hierarchy = _root.HierarchyAsString();
        }

        private TemplateContainer CreateRowTemplate() => _listRow.visualTreeAsset.CloneTree();

        private void DeleteEmployee(FluiBinder<Employee, VisualElement> row, FluiBinder<Employee, Button> x)
        {
            FluiHelper.ExecuteAfterClassTransition(row.Element, "transparent", "opacity", () => _employees.Remove(x.Context));
        }

        private void AddRandomEmployee()
        {
            _employees.Add(new Employee
            {
                Name = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6),
                Title = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 3),
                Salary = Random.Range(1, 6)
            });
        }

        private void Connect()
        {
            if (_document == null || _rootVisualElement == null)
            {
                _document = GetComponent<UIDocument>();
                _rootVisualElement = _document.rootVisualElement;
            }
        }

        private readonly List<Employee> _employees = new()
        {
            new Employee { Name = "Arne", Title = "Chimney Sweep", Salary = 7 },
            new Employee { Name = "Benny", Title = "Chimney Sweep", Salary = 3 },
            new Employee { Name = "Steve", Title = "Stevedore", Salary = 6 },
            new Employee { Name = "John", Title = "Yeoman", Salary = 2 }
        };

        private float GetSalarySum() => _employees.Sum(x => x.Salary);

        private class Employee
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public float Salary { get; set; }
        }
    }
}