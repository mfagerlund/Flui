// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using Flui;
using Flui.Binder;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace FluiDemo.ListUi.Binder
{
    public class ListUi : Fadable
    {
        private UIDocument _document;
        [SerializeField] private bool _rebuild;
        [SerializeField] private bool _pause;
        [SerializeField] public string _hierarchy;

        private FluiBinderRoot<ListUi, VisualElement> _root = new();

        private Action _onHide;

        private void Update()
        {
            Bind();
        }

        private void Bind()
        {
            if (_rebuild)
            {
                _root = new FluiBinderRoot<ListUi, VisualElement>();
                _rebuild = false;
            }

            _root.BindGui(this, RootVisualElement, r => r
                .Button("AddOffice", _ => AddOffice())
                .Button("Close", _ => Hide())
                .ForEach(
                    "Offices",
                    x => x._offices,
                    office => office
                        .Label("OfficeName", x => x.Name)
                        .Button("DeleteOffice", x => DeleteOffice(office.Element, office.Context))
                        .Group("List", x => x, b => b
                            .Button("Add", x => office.Context.AddRandomEmployee())
                            .ForEach(
                                "Content",
                                x => x.Employees,
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

        private readonly List<Office> _offices = Office.CreateOfficeList();
    }
}