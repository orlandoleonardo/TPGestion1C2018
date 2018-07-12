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
    public partial class frmGenerarReserva : Form
    {
        public frmGenerarReserva()
        {
            InitializeComponent();
        }

        private void frmGenerarReserva_Load(object sender, EventArgs e)
        {
            this.CenterToScreen();

            //cargo hoteles
            //si es recepción solo cargo el hotel donde trabaja
            if (Globals.rolUsuario == "Guest" || Globals.rolUsuario == "Administrador")
            {
                Utils.cargarHoteles(comboBoxHotel);
                comboBoxHotel.SelectedIndex = 0;
            }
            else
            {
                SqlCommand cargarHotelUsuario = new SqlCommand("SELECT nombre FROM [PISOS_PICADOS].Hotel WHERE idHotel = @id", Globals.conexionGlobal);
                cargarHotelUsuario.Parameters.Add("@id", SqlDbType.Int);
                cargarHotelUsuario.Parameters["@id"].Value = Globals.idHotelUsuario;
                //recibo nombre hotel
                try
                {
                    string nombreHotel = cargarHotelUsuario.ExecuteScalar().ToString();
                    comboBoxHotel.Items.Add(nombreHotel);
                    comboBoxHotel.SelectedIndex = 0;
                }
                catch
                {
                    MessageBox.Show("Error al cargar el hotel del usuario. Reinicie sesión y vuelva a intentar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            //agrego item por si no sabe el régimen que quiere
            comboBoxRegimen.Items.Add("Vacío");

            //busco regímenes
            Utils.cargarRegimenes(comboBoxRegimen);
            comboBoxRegimen.SelectedIndex = 0;
        }

        private void cargarPrecios()
        {
            int idHotel = 0;

            SqlCommand buscarIdHotel = new SqlCommand("SELECT idHotel FROM [PISOS_PICADOS].Hotel WHERE nombre = @nombreHotel", Globals.conexionGlobal);
            buscarIdHotel.Parameters.Add("@nombreHotel", SqlDbType.VarChar);
            buscarIdHotel.Parameters["@nombreHotel"].Value = comboBoxHotel.Text;
        
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

        private void comboBoxRegimen_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarPrecios();
        }

        private void comboBoxHotel_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarPrecios();
        }

        private void btnCrear_Click(object sender, EventArgs e)
        {
            int idCliente = 0;
            int codigoReservaNuevo = 0;

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

            int hotelCumple = 0;
            int idHotel = 0;
            //busco idHotel
            SqlCommand buscarIdHotel = new SqlCommand("SELECT idHotel FROM [PISOS_PICADOS].Hotel WHERE nombre = @nombreHotel", Globals.conexionGlobal);
            buscarIdHotel.Parameters.Add("@nombreHotel", SqlDbType.VarChar);
            buscarIdHotel.Parameters["@nombreHotel"].Value = comboBoxHotel.Text;

            idHotel = (int)buscarIdHotel.ExecuteScalar();

            //ejecuto función para ver si cumple lo demandado
            SqlCommand cmdhotelCumple = new SqlCommand("SELECT [PISOS_PICADOS].hotelCumple (@cantSimple, @cantDoble, @cantTriple, @cantCuadru, @cantKing, @idHotel, @fechaInicio, @fechaFin)", Globals.conexionGlobal);
            cmdhotelCumple.Parameters.Add("@cantSimple", SqlDbType.VarChar);
            cmdhotelCumple.Parameters.Add("@cantDoble", SqlDbType.VarChar);
            cmdhotelCumple.Parameters.Add("@cantTriple", SqlDbType.VarChar);
            cmdhotelCumple.Parameters.Add("@cantCuadru", SqlDbType.VarChar);
            cmdhotelCumple.Parameters.Add("@cantKing", SqlDbType.VarChar);
            cmdhotelCumple.Parameters.Add("@idHotel", SqlDbType.VarChar);
            cmdhotelCumple.Parameters.Add("@fechaInicio", SqlDbType.VarChar);
            cmdhotelCumple.Parameters.Add("@fechaFin", SqlDbType.VarChar);

            cmdhotelCumple.Parameters["@cantSimple"].Value = numSimple.Value;
            cmdhotelCumple.Parameters["@cantDoble"].Value = numDoble.Value;
            cmdhotelCumple.Parameters["@cantTriple"].Value = numTriple.Value;
            cmdhotelCumple.Parameters["@cantCuadru"].Value = numCuadruple.Value;
            cmdhotelCumple.Parameters["@cantKing"].Value = numKing.Value;
            cmdhotelCumple.Parameters["@idHotel"].Value = idHotel;
            cmdhotelCumple.Parameters["@fechaInicio"].Value = dtpInicioReserva.Value.ToString("yyyy-MM-dd");
            cmdhotelCumple.Parameters["@fechaFin"].Value = dtpFinReserva.Value.ToString("yyyy-MM-dd");

            //ejecuto y recibo resultado
            int resultadoBusqueda = (int)cmdhotelCumple.ExecuteScalar();

            //según resultado aviso al usuario
            if (resultadoBusqueda == 0)
            {
                DialogResult dialogResult = MessageBox.Show("Existe disponibilidad para la reserva solicitada. ¿Desea registrarla?", "Disponibilidad", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    //calculo cantidad huéspedes
                    int cantHuespedes = (int)numSimple.Value + (int)numDoble.Value * 2 + (int)numTriple.Value * 3 + (int)numCuadruple.Value * 4 + (int)numKing.Value * 5;
                    //efectúo la reserva
                    string spRegistrarReserva = "[PISOS_PICADOS].registrarReserva";
                    SqlCommand registrarReserva = new SqlCommand(spRegistrarReserva, Globals.conexionGlobal);
                    registrarReserva.CommandType = CommandType.StoredProcedure;

                    registrarReserva.Parameters.Add("@fechaReserva", SqlDbType.Date);
                    registrarReserva.Parameters.Add("@fechaInicio", SqlDbType.Date);
                    registrarReserva.Parameters.Add("@fechaFin", SqlDbType.Date);
                    registrarReserva.Parameters.Add("@cantHuespedes", SqlDbType.Int);
                    registrarReserva.Parameters.Add("@regimen", SqlDbType.VarChar);
                    registrarReserva.Parameters.Add("@idCliente", SqlDbType.Int);
                    registrarReserva.Parameters.Add("@idHotel", SqlDbType.Int);
                    registrarReserva.Parameters.Add("@cantSimple", SqlDbType.Int);
                    registrarReserva.Parameters.Add("@cantDoble", SqlDbType.Int);
                    registrarReserva.Parameters.Add("@cantTriple", SqlDbType.Int);
                    registrarReserva.Parameters.Add("@cantCuadru", SqlDbType.Int);
                    registrarReserva.Parameters.Add("@cantKing", SqlDbType.Int);

                    registrarReserva.Parameters["@fechaReserva"].Value = Globals.FechaDelSistema;
                    registrarReserva.Parameters["@fechaInicio"].Value = dtpInicioReserva.Value.ToString("yyyy-MM-dd");
                    registrarReserva.Parameters["@fechaFin"].Value = dtpFinReserva.Value.ToString("yyyy-MM-dd");
                    registrarReserva.Parameters["@cantHuespedes"].Value = cantHuespedes;
                    registrarReserva.Parameters["@regimen"].Value = comboBoxRegimen.Text;
                    registrarReserva.Parameters["@idCliente"].Value = idCliente;
                    registrarReserva.Parameters["@idHotel"].Value = idHotel;
                    registrarReserva.Parameters["@cantSimple"].Value = numSimple.Value;
                    registrarReserva.Parameters["@cantDoble"].Value = numDoble.Value;
                    registrarReserva.Parameters["@cantTriple"].Value = numTriple.Value;
                    registrarReserva.Parameters["@cantCuadru"].Value = numCuadruple.Value;
                    registrarReserva.Parameters["@cantKing"].Value = numKing.Value;

                    try
                    {
                        registrarReserva.ExecuteNonQuery();
                        MessageBox.Show("Reserva registrada correctamente. Su código de reserva es :" + codigoReservaNuevo + ". Le servirá para modificaciones y para hacer el ingreso en el hotel.", "Reserva", MessageBoxButtons.OK);
                    }
                    catch
                    {
                        MessageBox.Show("Error al registrar la reserva.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
}
