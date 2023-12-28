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
            _onHide?.Invoke();
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

        private void Bind()
        {
            Connect();
            if (_rebuild)
            {
                _root = new FluiBinderRoot<ListUi, VisualElement>();
                _rebuild = false;
            }

            _root.BindGui(this, _rootVisualElement, r => r
                .Button("AddOffice", _ => AddOffice())
                .Button("Close", _ => Hide())
                .ForEach(
                    "Offices",
                    x => x._offices,
                    "OfficeUxml",
                    office => office
                        .Label("OfficeName", x => x.Name)
                        .Button("DeleteOffice", x => DeleteOffice(office.Element, office.Context))
                        .Group("List", x => x, b => b
                            .Button("Add", x => office.Context.AddRandomEmployee())
                            .ForEach(
                                "Content",
                                x => x.Employees,
                                "EmployeeUxml",
                                row => row
                                    .Label("Name", x => x.Name)
                                    .Label("Title", x => x.Title)
                                    .Label("Salary", x => $"{x.Salary:0}")
                                    .Button("Delete", x => { DeleteEmployee(row, office.Context, x); })
                            ))
                        .Group("Footer", x => x, g => g
                            .Label("Salary", ctx => $"{g.Context.GetSalarySum():0}")
                        ))
            );

            _hierarchy = _root.HierarchyAsString();
        }

        private void DeleteOffice(VisualElement officeElement, Office office)
        {
            FluiHelper.ExecuteAfterClassTransition(
                officeElement,
                "transparent",
                "opacity",
                () => _offices.Remove(office));
        }

        private void AddOffice()
        {
            var office =
                new Office
                {
                    Name = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6),
                };
            _offices.Add(office);

            for (int i = 0; i < Random.Range(1, 4); i++)
            {
                office.AddRandomEmployee();
            }
        }

        private void DeleteEmployee(
            FluiBinder<Employee, VisualElement> row,
            Office office,
            FluiBinder<Employee, Button> x)
        {
            FluiHelper.ExecuteAfterClassTransition(
                row.Element,
                "transparent",
                "opacity",
                () => office.Employees.Remove(x.Context));
        }

        private void Connect()
        {
            if (_document == null || _rootVisualElement == null)
            {
                _document = GetComponent<UIDocument>();
                _rootVisualElement = _document.rootVisualElement;
            }
        }

        private readonly List<Office> _offices = new()
        {
            new Office
            {
                Name = "Central",
                Employees = new()
                {
                    new Employee { Name = "Arne", Title = "Chimney Sweep", Salary = 7 },
                    new Employee { Name = "Benny", Title = "Chimney Sweep", Salary = 3 },
                    new Employee { Name = "Steve", Title = "Stevedore", Salary = 6 },
                    new Employee { Name = "John", Title = "Yeoman", Salary = 2 }
                }
            },
            new Office
            {
                Name = "South",
                Employees = new()
                {
                    new Employee { Name = "Schultz", Title = "Chimney Sweep", Salary = 7 },
                    new Employee { Name = "Frantz", Title = "Chimney Sweep", Salary = 3 },
                    new Employee { Name = "Hanz", Title = "Stevedore", Salary = 6 },
                    new Employee { Name = "Salander", Title = "Yeoman", Salary = 2 }
                }
            }
        };

        private class Employee
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public float Salary { get; set; }
        }

        private class Office
        {
            public string Name { get; set; }
            public List<Employee> Employees { get; set; } = new();

            public float GetSalarySum() => Employees.Sum(x => x.Salary);

            private void DeleteEmployee(Employee employee) => Employees.Remove(employee);

            public void AddRandomEmployee()
            {
                Employees.Add(new Employee
                {
                    Name = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6),
                    Title = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 3),
                    Salary = Random.Range(1, 6)
                });
            }
        }
    }
}