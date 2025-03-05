# Manual de Usuario - AutoGest Pro

## 1. Introducción

### 1.1 Bienvenido a AutoGest Pro
AutoGest Pro es un sistema integral de gestión para talleres de reparación de vehículos. Diseñado para simplificar y optimizar las operaciones diarias, este software le ayudará a administrar vehículos, servicios, repuestos y facturación de manera eficiente.

## 2. Requisitos del Sistema
- Sistema Operativo: Linux
- Navegador o Interfaz Gráfica: Compatible con GTK

## 3. Inicio de Sesión

### 3.1 Acceso al Sistema
1. Abra la aplicación AutoGest Pro
2. Ingrese su correo electrónico
3. Introduzca su contraseña
4. Haga clic en "Iniciar Sesión"

### 3.2 Credenciales de Administrador
- **Usuario:** root@gmail.com
- **Contraseña:** root123

**Nota:** El usuario root tiene acceso completo a todas las funcionalidades del sistema.

## 4. Funcionalidades para Usuarios Root

### 4.1 Carga Masiva de Datos
#### 4.1.1 Carga de Usuarios
1. Seleccione "Carga Masiva"
2. Elija "Usuarios"
3. Seleccione el archivo JSON con los datos
4. Confirme la carga

#### 4.1.2 Carga de Vehículos
1. Seleccione "Carga Masiva"
2. Elija "Vehículos"
3. Seleccione el archivo JSON con los datos
4. Confirme la carga

#### 4.1.3 Carga de Repuestos
1. Seleccione "Carga Masiva"
2. Elija "Repuestos"
3. Seleccione el archivo JSON con los datos
4. Confirme la carga

### 4.2 Ingreso Manual de Datos
#### 4.2.1 Registro de Usuarios
1. Vaya a "Gestión de Usuarios"
2. Seleccione "Nuevo Usuario"
3. Complete los campos:
   - Nombres
   - Apellidos
   - Correo
   - Contraseña
4. Guarde el registro

#### 4.2.2 Registro de Vehículos
1. Vaya a "Gestión de Vehículos"
2. Seleccione "Nuevo Vehículo"
3. Complete los campos:
   - ID de Usuario
   - Marca
   - Modelo
   - Placa
4. Guarde el registro

#### 4.2.3 Registro de Repuestos
1. Vaya a "Gestión de Repuestos"
2. Seleccione "Nuevo Repuesto"
3. Complete los campos:
   - Nombre del Repuesto
   - Detalles
   - Costo
4. Guarde el registro

### 4.3 Gestión de Usuarios
#### 4.3.1 Ver Usuarios
1. Seleccione "Ver Usuarios"
2. Visualice la lista completa de usuarios
3. Haga clic en un usuario para ver detalles completos, incluyendo sus vehículos

#### 4.3.2 Editar Usuarios
1. Seleccione "Editar Usuario"
2. Busque por ID
3. Modifique:
   - Nombres
   - Apellidos
   - Correo
4. Guarde los cambios

#### 4.3.3 Eliminar Usuarios
1. Seleccione "Eliminar Usuario"
2. Ingrese el ID del usuario
3. Confirme la eliminación

### 4.4 Generación de Servicios
1. Vaya a "Generar Servicio"
2. Seleccione un vehículo existente
3. Elija un repuesto
4. Ingrese detalles del servicio
5. Confirme la generación

### 4.5 Facturación
#### 4.5.1 Generación Automática
- Al generar un servicio, el sistema crea automáticamente una factura
- La factura incluye:
  - ID
  - ID de Orden
  - Total (Costo del servicio + Costo del repuesto)

#### 4.5.2 Cancelación de Factura
1. Seleccione "Cancelar Factura"
2. El sistema mostrará la factura pendiente
3. Confirme el pago

## 5. Reportes

### 5.1 Tipos de Reportes
- Usuarios
- Vehículos
- Repuestos
- Servicios
- Facturación
- Bitácora
- Top 5 vehículos con más servicios
- Top 5 vehículos más antiguos

### 5.2 Generación de Reportes
1. Vaya a "Reportes"
2. Seleccione el tipo de reporte
3. Visualice o exporte según sea necesario

## 6. Consejos y Mejores Prácticas
- Mantenga sus credenciales seguras
- Realice copias de seguridad regularmente
- Verifique la información antes de cargarla
- Utilice la carga masiva para grandes volúmenes de datos

## 7. Solución de Problemas
- Si encuentra errores al cargar datos, verifique el formato JSON
- En caso de problemas de acceso, contacte al administrador
- Asegúrese de tener los permisos adecuados para cada acción

## 8. Contacto de Soporte
Para soporte técnico, comuníquese con el equipo de desarrollo de la Universidad San Carlos de Guatemala.

**¡Gracias por utilizar AutoGest Pro!**
