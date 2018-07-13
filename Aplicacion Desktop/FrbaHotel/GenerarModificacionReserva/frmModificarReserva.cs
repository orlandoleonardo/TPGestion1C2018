﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FrbaHotel.GenerarModificacionReserva
{
    public partial class frmModificarReserva : Form
    {
        int idCliente;
        public void setCliente(int id)
        {
            idCliente = id;
        }

        public frmModificarReserva()
        {
            InitializeComponent();
        }

        public frmModificarReserva(string codigoReserva)
        {
            InitializeComponent();
            labelCodigoReserva.Text = codigoReserva;
        }

        private void frmModificarReserva_Load(object sender, EventArgs e)
        {
            idCliente = Globals.idUsuarioSesion;

            this.CenterToScreen();

            //cargo hoteles
            SqlCommand cargarHotelReserva = new SqlCommand("SELECT nombre FROM [PISOS_PICADOS].Hotel WHERE idHotel IN (SELECT [PISOS_PICADOS].hotelDeReserva (@codigoReserva))", Globals.conexionGlobal);
            cargarHotelReserva.Parameters.Add("@codigoReserva", SqlDbType.Int);
            cargarHotelReserva.Parameters["@codigoReserva"].Value = Int64.Parse(labelCodigoReserva.Text);
            //recibo nombre hotel
            try
            {
                string nombreHotel = cargarHotelReserva.ExecuteScalar().ToString();
                labelHotel.Text = nombreHotel;
                }
            catch
            {
                MessageBox.Show("Error al cargar el hotel del usuario. Reinicie sesión y vuelva a intentar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //agrego item por si no sabe el régimen que quiere
            comboBoxRegimen.Items.Add("Vacío");

            //busco regímenes
            Utils.cargarRegimenes(comboBoxRegimen);
            comboBoxRegimen.SelectedIndex = 0;
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            numSimple.Value = 0;
            numDoble.Value = 0;
            numTriple.Value = 0;
            numCuadruple.Value = 0;
            numKing.Value = 0;
            dtpInicioReserva.ResetText();
            dtpFinReserva.ResetText();
            comboBoxRegimen.SelectedIndex = 0;
        }

        private void cargarPrecios()
        {
            int idHotel = 0;

            SqlCommand buscarIdHotel = new SqlCommand("SELECT idHotel FROM [PISOS_PICADOS].Hotel WHERE nombre = @nombreHotel", Globals.conexionGlobal);
            buscarIdHotel.Parameters.Add("@nombreHotel", SqlDbType.VarChar);
            buscarIdHotel.Parameters["@nombreHotel"].Value = labelHotel.Text;

            idHotel = (int)buscarIdHotel.ExecuteScalar();

            //si no eligió régimen traigo todos los precios. Si eligió, solo para ese régimen
            string query;
            if (comboBoxRegimen.Text == "Vacío")
            {
                query = "SELECT * FROM [PISOS_PICADOS].precioHabitacionesHotel (@idHotel)";
            }
            else
            {
                query = "SELECT * FROM [PISOS_PICADOS].precioHabitacionesHotel (@idHotel) WHERE [Tipo Regimen] = " + "'" + comboBoxRegimen.Text + "'";
            }

            SqlCommand preciosHotel = new SqlCommand(query, Globals.conexionGlobal);
            preciosHotel.Parameters.Add("@idHotel", SqlDbType.VarChar);
            preciosHotel.Parameters["@idHotel"].Value = idHotel;

            DataTable dtPrecios = new DataTable();
            SqlDataReader reader = preciosHotel.ExecuteReader();
            dtPrecios.Load(reader);
            reader.Close();
            dgvPrecios.DataSource = dtPrecios;
        }

        private void comboBoxHotel_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarPrecios();
        }

        private void comboBoxRegimen_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarPrecios();
        }

        public void volver(AbmCliente.frmAlta instanciaAlta)
        {
            instanciaAlta.Close();
            MessageBox.Show("Gracias por identificarse. Ya puede realizar la reserva.", "Reserva", MessageBoxButtons.OK);
            this.Show();
        }

        public void volver(frmSeleccionarCliente instanciaSeleccionarCliente)
        {
            instanciaSeleccionarCliente.Close();
            MessageBox.Show("Gracias por identificarse. Ya puede realizar la reserva.", "Reserva", MessageBoxButtons.OK);
            this.Show();
        }

        private void dtpInicioReserva_ValueChanged(object sender, EventArgs e)
        {
            if (dtpFinReserva.Value < dtpInicioReserva.Value) dtpFinReserva.Value = dtpInicioReserva.Value;
        }

        private void dtpFinReserva_ValueChanged(object sender, EventArgs e)
        {
            if (dtpInicioReserva.Value > dtpFinReserva.Value) dtpInicioReserva.Value = dtpFinReserva.Value;
        }

        private void btnCrear_Click(object sender, EventArgs e)
        {
            {
                //chequeos

                if (comboBoxRegimen.Text == "Vacío")
                {
                    MessageBox.Show("Debe elegir un régimen.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (numSimple.Value == 0 && numDoble.Value == 0 && numTriple.Value == 0 && numCuadruple.Value == 0 && numKing.Value == 0)
                {
                    MessageBox.Show("Debe elegir por lo menos una habitación.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //fin chequeos

                if (idCliente == -1)
                {
                    DialogResult dialogResult = MessageBox.Show("Debe identificarse o registrarse en el sistema para poder modificar una reserva. ¿Desea hacerlo?", "Estimado cliente", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        procesoInicioSesion();
                        return;
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                }

                //ejecuto función para ver si cumple lo demandado
                SqlCommand cmdPuedeModificarReserva = new SqlCommand("SELECT [PISOS_PICADOS].puedeModificarReserva (@fechaInicio, @fechaFin, @idReserva, @cantSimple, @cantDoble, @cantTriple, @cantCuadru, @cantKing)", Globals.conexionGlobal);
                cmdPuedeModificarReserva.Parameters.Add("@fechaInicio", SqlDbType.Date);
                cmdPuedeModificarReserva.Parameters.Add("@fechaFin", SqlDbType.Date);
                cmdPuedeModificarReserva.Parameters.Add("@idReserva", SqlDbType.Int);
                cmdPuedeModificarReserva.Parameters.Add("@cantSimple", SqlDbType.Int);
                cmdPuedeModificarReserva.Parameters.Add("@cantDoble", SqlDbType.Int);
                cmdPuedeModificarReserva.Parameters.Add("@cantTriple", SqlDbType.Int);
                cmdPuedeModificarReserva.Parameters.Add("@cantCuadru", SqlDbType.Int);
                cmdPuedeModificarReserva.Parameters.Add("@cantKing", SqlDbType.Int);

                cmdPuedeModificarReserva.Parameters["@fechaInicio"].Value = dtpInicioReserva.Value.ToString("yyyy-MM-dd");
                cmdPuedeModificarReserva.Parameters["@fechaFin"].Value = dtpFinReserva.Value.ToString("yyyy-MM-dd");
                cmdPuedeModificarReserva.Parameters["@idReserva"].Value = Int64.Parse(labelCodigoReserva.Text);
                cmdPuedeModificarReserva.Parameters["@cantSimple"].Value = numSimple.Value;
                cmdPuedeModificarReserva.Parameters["@cantDoble"].Value = numDoble.Value;
                cmdPuedeModificarReserva.Parameters["@cantTriple"].Value = numTriple.Value;
                cmdPuedeModificarReserva.Parameters["@cantCuadru"].Value = numCuadruple.Value;
                cmdPuedeModificarReserva.Parameters["@cantKing"].Value = numKing.Value;
                
                //ejecuto y recibo resultado
                int resultadoBusqueda = (int)cmdPuedeModificarReserva.ExecuteScalar();

                //según resultado aviso al usuario
                if (resultadoBusqueda == 0)
                {
                    DialogResult dialogResult = MessageBox.Show("Existe disponibilidad para la modificación solicitada. ¿Desea concretarla?", "Disponibilidad", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        //calculo cantidad huéspedes
                        int cantHuespedes = (int)numSimple.Value + (int)numDoble.Value * 2 + (int)numTriple.Value * 3 + (int)numCuadruple.Value * 4 + (int)numKing.Value * 5;
                        //efectúo la reserva
                        string spModificarReserva = "[PISOS_PICADOS].modificarReserva";
                        SqlCommand modificarReserva = new SqlCommand(spModificarReserva, Globals.conexionGlobal);
                        modificarReserva.CommandType = CommandType.StoredProcedure;

                        modificarReserva.Parameters.Add("@fechaModificacion", SqlDbType.Date);
                        modificarReserva.Parameters.Add("@fechaInicio", SqlDbType.Date);
                        modificarReserva.Parameters.Add("@fechaFin", SqlDbType.Date);
                        modificarReserva.Parameters.Add("@cantHuespedes", SqlDbType.Int);
                        modificarReserva.Parameters.Add("@nombreRegimen", SqlDbType.VarChar);
                        modificarReserva.Parameters.Add("@idReserva", SqlDbType.Int);
                        modificarReserva.Parameters.Add("@idAutor", SqlDbType.Int);
                        modificarReserva.Parameters.Add("@cantSimple", SqlDbType.Int);
                        modificarReserva.Parameters.Add("@cantDoble", SqlDbType.Int);
                        modificarReserva.Parameters.Add("@cantTriple", SqlDbType.Int);
                        modificarReserva.Parameters.Add("@cantCuadru", SqlDbType.Int);
                        modificarReserva.Parameters.Add("@cantKing", SqlDbType.Int);
                        modificarReserva.Parameters.Add("@motivo", SqlDbType.VarChar);

                        modificarReserva.Parameters["@fechaModificacion"].Value = Globals.FechaDelSistema;
                        modificarReserva.Parameters["@fechaInicio"].Value = dtpInicioReserva.Value.ToString("yyyy-MM-dd");
                        modificarReserva.Parameters["@fechaFin"].Value = dtpFinReserva.Value.ToString("yyyy-MM-dd");
                        modificarReserva.Parameters["@cantHuespedes"].Value = cantHuespedes;
                        modificarReserva.Parameters["@nombreRegimen"].Value = comboBoxRegimen.Text;
                        modificarReserva.Parameters["@idReserva"].Value = Int64.Parse(labelCodigoReserva.Text);
                        modificarReserva.Parameters["@idAutor"].Value = idCliente;
                        modificarReserva.Parameters["@cantSimple"].Value = numSimple.Value;
                        modificarReserva.Parameters["@cantDoble"].Value = numDoble.Value;
                        modificarReserva.Parameters["@cantTriple"].Value = numTriple.Value;
                        modificarReserva.Parameters["@cantCuadru"].Value = numCuadruple.Value;
                        modificarReserva.Parameters["@cantKing"].Value = numKing.Value;
                        modificarReserva.Parameters["@motivo"].Value = txtMotivo.Text;

                        try
                        {
                            modificarReserva.ExecuteNonQuery();
                            MessageBox.Show("Reserva modificada correctamente.", "Reserva", MessageBoxButtons.OK);
                        }
                        catch
                        {
                            MessageBox.Show("Error al modificar la reserva.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                }
                else if (resultadoBusqueda == 1)
                {
                    MessageBox.Show("No hay disponibilidad de esa cantidad de habitaciones simples.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (resultadoBusqueda == 2)
                {
                    MessageBox.Show("No hay disponibilidad de esa cantidad de habitaciones dobles.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (resultadoBusqueda == 3)
                {
                    MessageBox.Show("No hay disponibilidad de esa cantidad de habitaciones triples.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (resultadoBusqueda == 4)
                {
                    MessageBox.Show("No hay disponibilidad de esa cantidad de habitaciones cuádruples.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (resultadoBusqueda == 5)
                {
                    MessageBox.Show("No hay disponibilidad de esa cantidad de habitaciones king.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void procesoInicioSesion()
        {
            DialogResult dialogResult = MessageBox.Show("¿Ya se registró previamente en el sistema?", "Identificación", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                frmSeleccionarCliente seleccionarCliente = new frmSeleccionarCliente(this);
                seleccionarCliente.Show();
            }
            else if (dialogResult == DialogResult.No)
            {
                AbmCliente.frmAlta instanciafrmCliente = new AbmCliente.frmAlta(this);
                instanciafrmCliente.Show();
                this.Hide();
                Globals.frmMenuInstance.Hide();
            }
        }
    }
}
