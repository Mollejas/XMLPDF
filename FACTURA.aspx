


<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="FACTURA.aspx.vb" Inherits="FACT.FACTURA" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>PANINO</title>
  <!-- Bootstrap CSS -->
  <link rel="stylesheet" href="https://unpkg.com/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
  <!-- jQuery UI CSS -->
  <link rel="stylesheet" href="https://code.jquery.com/ui/1.13.2/themes/base/jquery-ui.css"/>
  <!-- Bootstrap Icons -->
  <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css">
  <style>
    body {
      background: linear-gradient(135deg, #cce5ff, #f8d7da);
      font-size: 0.85rem;
      margin: 0;
      padding: 0;
    }

    /* Sidebar Styles */
    .sidebar {
      background-color: #000;
      color: #fff;
      width: 250px;
      height: 100vh;
      position: fixed;
      left: 0;
      top: 0;
      z-index: 1000;
      padding: 20px 0;
      box-shadow: 2px 0 5px rgba(0,0,0,0.1);
    }

    .sidebar-header {
      text-align: center;
      padding: 20px;
      border-bottom: 1px solid #333;
      margin-bottom: 20px;
    }

    .sidebar-header h4 {
      color: #fff;
      margin: 0;
      font-weight: bold;
    }

    .sidebar-menu {
      list-style: none;
      padding: 0;
      margin: 0;
    }

    .sidebar-menu li {
      margin: 0;
    }

    .sidebar-menu a {
      display: block;
      color: #fff;
      text-decoration: none;
      padding: 15px 25px;
      transition: all 0.3s ease;
      border-left: 3px solid transparent;
    }

    .sidebar-menu a:hover {
      background-color: #333;
      border-left-color: #0056b3;
      color: #fff;
    }

    .sidebar-menu a.active {
      background-color: #0056b3;
      border-left-color: #fff;
    }

    .sidebar-menu i {
      margin-right: 10px;
      width: 20px;
      text-align: center;
    }

    /* Main content area */
    .main-content {
      margin-left: 250px;
      min-height: 100vh;
      padding: 20px;
    }

    /* Toggle button for mobile */
    .sidebar-toggle {
      display: none;
      position: fixed;
      top: 20px;
      left: 20px;
      z-index: 1001;
      background-color: #000;
      color: #fff;
      border: none;
      padding: 10px;
      border-radius: 5px;
      font-size: 18px;
    }

    /* Responsive */
    @media (max-width: 768px) {
      .sidebar {
        transform: translateX(-100%);
        transition: transform 0.3s ease;
      }

      .sidebar.show {
        transform: translateX(0);
      }

      .main-content {
        margin-left: 0;
      }

      .sidebar-toggle {
        display: block;
      }
    }

    /* Original styles */
    .card { border: none; border-radius: 0.5rem; }
    .card-header { background-color: #0056b3; color: #fff; font-weight: 600; }
    .form-label { font-weight: 500; }
    .form-control:focus, .form-select:focus {
      border-color: #dc3545;
      box-shadow: 0 0 0 .2rem rgba(220,53,69,.25);
    }
    .table thead { background-color: #0056b3; color: #fff; }

    .table tbody tr:nth-child(odd) { background-color: transparent; }
    .table tbody tr:nth-child(even) { background-color: transparent; }

    .table-sm th, .table-sm td { padding: 0.5rem; }
    .form-control-sm, .form-select-sm { padding: 0.4rem 0.6rem; }
    .g-2 { --bs-gutter-x: 0.5rem; --bs-gutter-y: 0.5rem; }
    .card-footer { background-color: #dc3545; color: #fff; padding: 0.75rem 1rem; }
    .footer-label { font-size: 3rem; font-weight: bold; color: #000; }
    .footer-text { font-size: 2rem; font-weight: bold; }
    .ui-autocomplete { max-height: 200px; overflow-y: auto; overflow-x: hidden; padding-right: 20px; z-index: 10000; }
    * html .ui-autocomplete { height: 200px; }

  </style>
</head>
<body>
  <form id="form1" runat="server" autocomplete="off">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <!-- Sidebar -->
    <div class="sidebar" id="sidebar">
      <div class="sidebar-header">
        <img src="images/logo.png" alt="LA CASA DEL AJUSTE DE MOTOR" class="sidebar-logo" style="max-width: 100%; height: auto; margin-bottom: 10px;" />
      </div>
      <ul class="sidebar-menu">
        <li>
          <a href="#" class="menu-item" data-section="cotizacion">
            <i class="bi bi-file-text"></i>
            COTIZACIÓN
          </a>
        </li>
        <li>
          <a href="#" class="menu-item" data-section="ticket">
            <i class="bi bi-receipt"></i>
            TICKET
          </a>
        </li>
        <li>
          <a href="#" class="menu-item" data-section="remision">
            <i class="bi bi-truck"></i>
            REMISIÓN
          </a>
        </li>
        <li>
          <a href="#" class="menu-item active" data-section="facturas">
            <i class="bi bi-file-earmark-text"></i>
            FACTURAS
          </a>
        </li>
      </ul>
    </div>

    <!-- Toggle button for mobile -->
    <button type="button" class="sidebar-toggle" id="sidebarToggle">
      <i class="bi bi-list"></i>
    </button>

    <!-- Main content -->
    <div class="main-content">
      <div class="container-fluid">
        <div class="card shadow">
          <div class="card-header text-center">LA CASA DEL AJUSTE DE MOTOR - FACTURAS</div>
          <div class="card-body">

            <!-- Hidden para pasar JSON al servidor -->
            <asp:HiddenField ID="hf_JSON_FACTURA" runat="server" ClientIDMode="Static" />

            <!-- Formulario -->
            <div class="row g-2">
              <div class="col-6 col-md-3">
                <label for="claveCliente" class="form-label">Clave Cliente</label>
                <input type="text" id="claveCliente" class="form-control form-control-sm" />
              </div>
              <div class="col-6 col-md-3">
                <label for="nombreCliente" class="form-label">Nombre Cliente</label>
                <input type="text" id="nombreCliente" class="form-control form-control-sm" readonly />
              </div>
              <div class="col-6 col-md-2">
                <label for="listaPrecios" class="form-label">Lista Precios</label>
                <input type="text" id="listaPrecios" class="form-control form-control-sm" />
              </div>
              <div class="col-6 col-md-2">
                <label for="numVendedor" class="form-label">Num Vendedor</label>
                <input type="text" id="numVendedor" class="form-control form-control-sm" />
              </div>
              <div class="col-6 col-md-2">
                <label for="nombreVendedor" class="form-label">Nombre Vendedor</label>
                <input type="text" id="nombreVendedor" class="form-control form-control-sm" />
              </div>
              <div class="col-4 col-md-2">
                <label for="usoCFDI" class="form-label">Uso CFDI</label>
                <select id="usoCFDI" class="form-select form-select-sm">
                  <option value="">--</option>
                  <option>G01</option>
                  <option>G03</option>
                  <option>P01</option>
                </select>
              </div>
              <div class="col-4 col-md-2">
                <label for="metodoPago" class="form-label">Método Pago</label>
                <select id="metodoPago" class="form-select form-select-sm">
                  <option value="">--</option>
                  <option>PUE</option>
                  <option>PPD</option>
                </select>
              </div>
              <div class="col-4 col-md-2">
                <label for="formaPago" class="form-label">Forma Pago</label>
                <select id="formaPago" class="form-select form-select-sm">
                  <option value="">--</option>
                  <option>EFECTIVO</option>
                  <option>CHEQUE</option>
                  <option>TRANSFERENCIA</option>
                  <option>TARJETA CRÉDITO</option>
                  <option>TARJETA DÉBITO</option>
                  <option>POR DEFINIR</option>
                </select>
              </div>
              <div class="col-6 col-md-2">
                <label for="obs" class="form-label">Obs</label>
                <input type="text" id="obs" class="form-control form-control-sm" />
              </div>
              <div class="col-6 col-md-2">
                <label for="obs1" class="form-label">Obs1</label>
                <input type="text" id="obs1" class="form-control form-control-sm" />
              </div>
              <div class="col-6 col-md-2">
                <label for="obs2" class="form-label">Obs2</label>
                <input type="text" id="obs2" class="form-control form-control-sm" />
              </div>
              <div class="col-6 col-md-2">
                <label for="obs3" class="form-label">Obs3</label>
                <input type="text" id="obs3" class="form-control form-control-sm" />
              </div>
            </div>

            <!-- Tabla de artículos -->
            <div class="table-responsive mt-2">
              <table class="table table-bordered table-hover table-sm table-striped" id="itemsTable">
                <colgroup>
                  <col style="width:9.29%">
                  <col style="width:71.14%">
                  <col style="width:6.52%">
                  <col style="width:6.52%">
                  <col style="width:6.52%">
                </colgroup>
                <thead>
                  <tr>
                    <th>Artículo</th>
                    <th>Descripción</th>
                    <th>Cantidad</th>
                    <th>Precio</th>
                    <th>Importe</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td><input type="text" class="form-control form-control-sm articulo" /></td>
                    <td><input type="text" class="form-control form-control-sm descripcion" /></td>
                    <td><input type="number" min="0" step="1" class="form-control form-control-sm text-end cantidad" /></td>
                    <td><input type="number" min="0" step="0.01" class="form-control form-control-sm text-end precio" /></td>
                    <td><input type="text" class="form-control form-control-sm text-end importe" readonly/></td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- Footer dentro de la tarjeta -->
          <div class="card-footer">
            <div class="d-flex justify-content-between align-items-center">
              <div class="d-flex flex-column">
                <span id="subtotalLabel" class="text-light">SubTotal: 0.00</span>
                <span id="ivaLabel" class="text-light">IVA: 0.00</span>
              </div>
              <div class="d-flex align-items-center">
                <div class="text-end me-3">
                  <div class="footer-text">Total:</div>
                  <span id="totalLabel" class="footer-label">0.00</span>
                </div>

                <!-- BOTÓN PARA GENERAR XML + PDF -->
                <asp:Button ID="btnGenerarXMLPDF" runat="server"
                    CssClass="btn btn-dark"
                    Text="Generar XML + PDF"
                    OnClick="btnGenerarXMLPDF_Click"
                    OnClientClick="prepararDatosFactura();" />
              </div>
            </div>
          </div>

        </div>
      </div>
    </div>

    <!-- Scripts -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://code.jquery.com/ui/1.13.2/jquery-ui.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>

    <script>
        // =========================
        // Función global para armar JSON de la factura (encabezado + conceptos)
        // =========================
        function prepararDatosFactura() {
            var factura = {
                clienteClave: $("#claveCliente").val(),
                clienteNombre: $("#nombreCliente").val(),
                lista: $("#listaPrecios").val(),
                vendedor: $("#numVendedor").val(),
                usoCFDI: $("#usoCFDI").val(),
                metodoPago: $("#metodoPago").val(),
                formaPago: $("#formaPago").val(),
                obs: $("#obs").val(),
                obs1: $("#obs1").val(),
                obs2: $("#obs2").val(),
                obs3: $("#obs3").val(),
                conceptos: []
            };

            $("#itemsTable tbody tr").each(function () {
                factura.conceptos.push({
                    clave: $(this).find(".articulo").val(),
                    descripcion: $(this).find(".descripcion").val(),
                    cantidad: parseFloat($(this).find(".cantidad").val()) || 0,
                    precio: parseFloat($(this).find(".precio").val()) || 0,
                    importe: parseFloat($(this).find(".importe").val()) || 0
                });
            });

            $("#hf_JSON_FACTURA").val(JSON.stringify(factura));
        }

        // =========================
        // Lógica original + totales
        // =========================
        function updateTotals() {
            let sub = 0;
            $('#itemsTable tbody tr').each(function () {
                sub += parseFloat($(this).find('.importe').val()) || 0;
            });
            const iva = sub * 0.16;
            $('#subtotalLabel').text(
                'SubTotal: ' +
                new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(sub)
            );
            $('#ivaLabel').text(
                'IVA: ' +
                new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(iva)
            );
            $('#totalLabel').text(
                new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(sub + iva)
            );
        }

        $(function () {
            // Sidebar functionality
            $('#sidebarToggle').on('click', function () {
                $('#sidebar').toggleClass('show');
            });

            // Menu item click handler
            $('.menu-item').on('click', function (e) {
                e.preventDefault();
                $('.menu-item').removeClass('active');
                $(this).addClass('active');

                const section = $(this).data('section');
                const titles = {
                    'cotizacion': 'COTIZACIÓN',
                    'ticket': 'TICKET',
                    'remision': 'REMISIÓN',
                    'facturas': 'FACTURAS'
                };

                $('.card-header').text('LA CASA DEL AJUSTE DE MOTOR - ' + titles[section]);

                if (window.innerWidth <= 768) {
                    $('#sidebar').removeClass('show');
                }
            });

            // Close sidebar when clicking outside on mobile
            $(document).on('click', function (e) {
                if (window.innerWidth <= 768) {
                    if (!$(e.target).closest('#sidebar, #sidebarToggle').length) {
                        $('#sidebar').removeClass('show');
                    }
                }
            });

            // Colores de filas alternadas
            function updateRowStripes() {
                $('#itemsTable tbody tr').each(function (index) {
                    if (index % 2 === 0) {
                        $(this).css('background-color', '#ffffff');
                    } else {
                        $(this).css('background-color', '#374151');
                    }
                });
            }

            const tableBody = $('#itemsTable tbody');

            function addRow() {
                const row = $('<tr>').append(
                    '<td><input type="text" class="form-control form-control-sm articulo" /></td>' +
                    '<td><input type="text" class="form-control form-control-sm descripcion" /></td>' +
                    '<td><input type="number" min="0" step="1" class="form-control form-control-sm text-end cantidad" /></td>' +
                    '<td><input type="number" min="0" step="0.01" class="form-control form-control-sm text-end precio" /></td>' +
                    '<td><input type="text" class="form-control form-control-sm text-end importe" readonly/></td>'
                );
                tableBody.append(row);
                updateRowStripes();
                attachEvents(row);
            }

            function attachEvents($row) {
                $row.find('.cantidad, .precio').on('input', function () {
                    const qty = parseFloat($row.find('.cantidad').val()) || 0;
                    const prc = parseFloat($row.find('.precio').val()) || 0;
                    $row.find('.importe').val((qty * prc).toFixed(2));
                    updateTotals();
                });
                $row.on('focusin', function () {
                    const rows = tableBody.find('tr');
                    if ($row.is(rows.last()) &&
                        $row.find('input').filter((i, el) => el.value).length) {
                        addRow();
                    }
                });
            }

            attachEvents(tableBody.find('tr').first());
            updateRowStripes();
            updateTotals();

            // Delete row con tecla SUPR en Artículo
            $(document).on('keydown', '.articulo', function (e) {
                if ((e.which || e.keyCode) === 46) {
                    e.preventDefault();
                    const $r = $(this).closest('tr');
                    if ($r.siblings().length) {
                        $r.remove();
                        updateRowStripes();
                    } else {
                        $r.find('input').val('');
                    }
                    updateTotals();
                }
            });

            // Navegación con ENTER
            $(document).on('keydown', 'input:visible:enabled:not([readonly]), select:visible:enabled', function (e) {
                if ((e.which || e.keyCode) === 13) {
                    e.preventDefault();
                    const f = $('input:visible:enabled:not([readonly]), select:visible:enabled')
                        .filter((i, el) => $(el).is('input[type=text], input[type=number], select'));
                    const idx = f.index(this);
                    if (idx > -1 && idx < f.length - 1) f.eq(idx + 1).focus();
                }
            });

            // Autocomplete Artículo
            $(document).on('focus', '.articulo', function () {
                const $i = $(this);
                if (!$i.data('ui-autocomplete')) {
                    $i.autocomplete({
                        source: function (req, res) {
                            $.ajax({
                                url: 'Autocomplete.ashx',
                                dataType: 'json',
                                data: { term: req.term },
                                success: res
                            });
                        },
                        minLength: 2,
                        select: function (e, ui) {
                            $i.val(ui.item.clave);
                            $i.closest('tr').find('.descripcion').val(ui.item.descripcion);
                            return false;
                        }
                    }).autocomplete('instance')._renderItem = function (ul, item) {
                        return $('<li>')
                            .append('<div>' + item.label + '</div>')
                            .appendTo(ul);
                    };
                }
            });

            // Enter + Price lookup
            $(document).on('keydown blur', '.articulo', function (e) {
                if (e.type === 'keydown' && (e.which || e.keyCode) !== 13) return;
                if (e.type === 'keydown') e.preventDefault();

                const $i = $(this),
                    row = $i.closest('tr'),
                    term = $i.val().trim(),
                    lista = $('#listaPrecios').val();

                if (!term) return;

                $.ajax({
                    url: 'Autocomplete.ashx',
                    dataType: 'json',
                    data: { term: term },
                    success: function (data) {
                        const m = data.find(it => it.clave.toLowerCase() === term.toLowerCase());
                        if (m) {
                            $i.val(m.clave);
                            row.find('.descripcion').val(m.descripcion).focus();
                        } else {
                            alert('Artículo no encontrado.');
                            $i.focus();
                        }
                    },
                    error: function () {
                        alert('Error al consultar.');
                        $i.focus();
                    }
                });

                if (lista) {
                    $.ajax({
                        url: 'PriceHandler.ashx',
                        dataType: 'json',
                        data: { clave: term, lista: lista },
                        success: function (d) {
                            const p = parseFloat(d.precio) || 0;
                            row.find('.precio').val(p.toFixed(2)).trigger('input');
                        },
                        error: function () { }
                    });
                }
            });

            // Autocomplete Cliente
            $('#claveCliente').autocomplete({
                source: function (req, resp) {
                    $.ajax({
                        url: 'ClienteAutocomplete.ashx',
                        dataType: 'json',
                        data: { term: req.term },
                        success: resp
                    });
                },
                minLength: 2,
                select: function (e, ui) {
                    $('#claveCliente').val(ui.item.clave);
                    $('#nombreCliente').val(ui.item.nombre);
                    return false;
                }
            }).autocomplete('instance')._renderItem = function (ul, item) {
                return $('<li>')
                    .append('<div>' + item.label + '</div>')
                    .appendTo(ul);
            };

            $('#claveCliente').keydown(function (e) {
                if ((e.which || e.keyCode) === 13) {
                    e.preventDefault();
                    const term = $(this).val().trim();
                    if (!term) return;
                    $.ajax({
                        url: 'ClienteAutocomplete.ashx',
                        dataType: 'json',
                        data: { term: term },
                        success: function (data) {
                            const m = data.find(it => it.clave.toLowerCase() === term.toLowerCase());
                            if (m) {
                                $('#claveCliente').val(m.clave);
                                $('#nombreCliente').val(m.nombre);
                                $('#numVendedor').focus();
                            } else {
                                alert('Cliente no encontrado.');
                                $('#claveCliente').focus();
                            }
                        },
                        error: function () {
                            alert('Error al consultar.');
                            $('#claveCliente').focus();
                        }
                    });
                }
            });
        });
    </script>
  </form>
</body>
</html>
