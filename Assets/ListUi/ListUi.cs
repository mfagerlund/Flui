// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using Flui.Binder;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace FluiDemo.ListUi
{
    public class ListUi : MonoBehaviour
    {
        private UIDocument _document;
        [SerializeField] private UIDocument _listRow;
        [SerializeField] public string _hierarchy;
        [SerializeField] private bool _pause;
        [SerializeField] private bool _rebuild;

        private FluiBinderRoot<ListUi, VisualElement> _root = new();

        private Action _onHide;

        public void Show(Action onHide)
        {
            _onHide = onHide;
            gameObject.SetActive(true);
        }

        private void Hide()
        {
            gameObject.SetActive(false);
            _onHide();
        }

        private void OnValidate()
        {
            if (Application.isPlaying || !gameObject.activeSelf) return;
            Bind();
        }

        private void Update()
        {
            Bind();
        }

        private void Bind()
        {
            if ((_document ??= _document = GetComponent<UIDocument>()) == null)
            {
                throw new InvalidOperationException("_document not assigned");
            }

            if (_rebuild)
            {
                _root = new FluiBinderRoot<ListUi, VisualElement>();
                _rebuild = false;
            }

            if (_document.rootVisualElement == null)
            {
                Debug.Log("_document.rootVisualElement is null");
                return;
            }

            _root.BindGui(this, _document.rootVisualElement, r => r
                .Button("Close", _ => Hide())
                .ForEach("Content", x => x._employees, () => _listRow.visualTreeAsset.CloneTree(), row => row
                    .Label("Name", x => x.Name)
                    .Label("Title", x => x.Title)
                    .Label("Salary", x => $"{x.Salary:0}")
                    .Button("Delete", x => { DeleteEmployee(row, x); })
                )
                .Group("Footer", x => x, g => g
                    .Label("Salary", ctx => $"{GetSalarySum():0}")
                )
                .Button("Add", x => AddRandomEmployee())
            );

            _hierarchy = _root.HierarchyAsString();
        }

        private void DeleteEmployee(FluiBinder<Employee, VisualElement> row, FluiBinder<Employee, Button> x)
        {
            row.Element.AddToClassList("transparent");
            row.Element.RegisterCallback<TransitionEndEvent>(
                e =>
                {
                    if (e.stylePropertyNames.Contains("opacity"))
                    {
                        _employees.Remove(x.Context);
                    }
                });
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

        private readonly List<Employee> _employees = new()
        {
            new Employee { Name = "Arne", Title = "Chimney Sweep", Salary = 7 },
            new Employee { Name = "Benny", Title = "Chimney Sweep", Salary = 3 },
            new Employee { Name = "Steve", Title = "Stevedore", Salary = 6 },
            new Employee { Name = "John", Title = "Yeoman", Salary = 2 }
        };

        private class Employee
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public float Salary { get; set; }
        }

        private float GetSalarySum() => _employees.Sum(x => x.Salary);
    }
}