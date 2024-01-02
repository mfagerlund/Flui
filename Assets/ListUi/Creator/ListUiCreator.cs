// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using Flui;
using Flui.Binder;
using Flui.Creator;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace FluiDemo.ListUi.Creator
{
    public class ListUiCreator : Fadable
    {
        private readonly List<Office> _offices = Office.CreateOfficeList();
        private FluiCreatorRoot<ListUiCreator, VisualElement> _root = new();

        private void Update()
        {
            Bind();
        }

        private void Bind()
        {
            _root.CreateGui(this, RootVisualElement, r => r
                .VisualElement("unnamed0", "row", unnamed0 => unnamed0
                    .Label("ListExamples", _ => "List Examples", "h2")
                    .Button("AddOffice", "Add Office", "btn-primary", _ => AddOffice())
                    .Button("Close", "Close", "btn-danger", _ => Hide())
                )
                .ScrollView("unnamed1", "", unnamed1 => unnamed1
                    .VisualElement("Root", "", root => root
                        .VisualElement("Offices", "", offices => offices
                            .VisualElement("b559cdb780fe4b5d8bd26996b3b78fa5", "", b559cdb780fe4b5d8bd26996b3b78fa5 => b559cdb780fe4b5d8bd26996b3b78fa5
                                .ForEach("Offices",
                                    x => x._offices,
                                    "",
                                    office => office
                                        .VisualElement("unnamed2", "row", unnamed2 => unnamed2
                                            .Label("Label", _ => "Office: ", "h3")
                                            .Label(x => x.Name, "h3")
                                            .Button("DeleteOffice", "Delete Office", "btn-warning", x => DeleteOffice(x.Element, x.Context))
                                        )
                                        .VisualElement("unnamed3", "", unnamed3 => unnamed3
                                            .VisualElement("List", "table", list => list
                                                .VisualElement("Header", "tr", header => header
                                                    .Label("Name", _ => "Name", "th")
                                                    .Label("Title", _ => "Title", "th")
                                                    .Label("Salary", _ => "Salary", "th")
                                                    .VisualElement("unnamed4", "", unnamed4 => unnamed4
                                                        .Button("Add", "Add", "btn-primary, btn-sm", _ => AddOffice())
                                                    )
                                                )
                                                .VisualElement("Content", "")
                                                .VisualElement("Footer", "tr", footer => footer
                                                    .Label("Name", _ => "", "td")
                                                    .Label("Title", _ => "", "td")
                                                    .Label("Salary", _ => "0", "td")
                                                    .VisualElement("unnamed5", "")
                                                )
                                            )
                                        )
                                )
                            )
                        )
                    )
                )
            );
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
    }
}