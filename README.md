# Tiny Sokoban
Un juego que reimplementa el clásico **Sokoban**, donde controlas a un pastor que debe guiar ovejas hasta sus posiciones objetivo.

---

## Características

- **Crear niveles** desde cero  
- **Editar niveles existentes**  
- **Jugar niveles personalizados**  
- Sistema de carga de niveles dinámico  
- Exportación a .txt

## Cómo jugar

El objetivo es simple: 
- Llevar todas las ovejas 🐑 a sus posiciones objetivo (**Goals**)

Pero cuidado:

- ❌ No puedes tirar hacia tí de las ovejas  
- ❌ Una oveja **no puede empujar a otra**  
- ❌ Puedes bloquearte si no planificas bien

### 🎮 Controles

Compatibilidad con Keyboard y Gamepad

| Tecla | Acción |
|------|--------|
| W | Mover arriba |
| A | Mover izquierda |
| S | Mover abajo |
| D | Mover derecha |

## Editor de niveles

El juego incluye un editor donde puedes:
- Crear mapas desde cero  
- Pintar:
  - Suelo simple  
  - Muros  
  - Objetivos  
  - Ovejas  
  - Jugador  
- Guardar niveles en formato `.txt`  
- Editar niveles ya existentes

### Formato de niveles (.txt)

| Símbolo | Significado |
|--------|------------|
| `#` | Muro |
| `.` | Suelo |
| `G` | Objetivo |
| `B` | Oveja |
| `P` | Jugador |
| `+` | Jugador sobre objetivo |
| `*` | Oveja sobre objetivo |

## Futuras mejoras

- Retroceder un movimiento durante un nivel en juego.
- Añadir animaciones a los Sprites.
- Añadir Música y efectos de sonidos.
- Añadir Menú de Opciones.
- Mejorar Aspecto y Interfaz del modo Editor de niveles.
- Sistema de puntuación (ver que niveles faltan por completar).
- Más niveles.
- Mejorar la distribución y estética del selector de niveles.
- Permitir borrar un nivel.
- Y mucho más...
