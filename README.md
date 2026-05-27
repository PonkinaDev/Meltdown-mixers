<a name="readme-top"></a>

[![Unity](https://img.shields.io/badge/Unity-6000.x-black?style=for-the-badge&logo=unity&logoColor=white)](https://unity.com/)
[![CSharp](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Photon Fusion 2](https://img.shields.io/badge/Photon_Fusion-2-0082C8?style=for-the-badge&logo=photon&logoColor=white)](https://www.photonengine.com/fusion)
[![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D4?style=for-the-badge&logo=windows&logoColor=white)](https://www.microsoft.com/windows)
[![TextMeshPro](https://img.shields.io/badge/TextMeshPro-UI-blueviolet?style=for-the-badge&logo=unity&logoColor=white)](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html)
[![Status](https://img.shields.io/badge/Status-Completed-brightgreen?style=for-the-badge)](.)

<br />

# Chemical Potions — Juego Multijugador Cooperativo de Pociones

Juego cooperativo en tiempo real de tipo *cocinero caótico*, desarrollado en Unity con **Photon Fusion 2** bajo la topología **Player Host**. Dos jugadores comparten una cocina mágica donde deben preparar y entregar pociones de colores antes de alcanzar la meta económica. El host actúa simultáneamente como servidor y jugador activo; no existe servidor dedicado.

---

## Tabla de Contenidos

- [Descripción General](#descripción-general)
- [Requisitos Técnicos](#requisitos-técnicos)
- [Cómo Ejecutar el Juego](#cómo-ejecutar-el-juego)
- [Controles](#controles)
- [Arquitectura de Red](#arquitectura-de-red)
- [Mecánicas de Juego](#mecánicas-de-juego)
- [Sistema de Ingredientes y Pociones](#sistema-de-ingredientes-y-pociones)
- [Selección de Avatar](#selección-de-avatar)
- [Flujo Completo del Juego](#flujo-completo-del-juego)
- [Estructura de Scripts](#estructura-de-scripts)
- [Integrantes](#integrantes)

---

## Descripción General

**Chemical Potions** es un juego cooperativo multijugador para exactamente 2 jugadores. Cada jugador controla un personaje dentro de una cocina mágica y debe colaborar para recoger ingredientes, mezclarlos en estaciones especializadas, cocinarlos correctamente y entregarlos al mostrador de pedidos. El objetivo es acumular **$300** antes de que el caos se apodere de la cocina.

El modelo de red es **Player Host con Photon Fusion 2**: quien crea la sala lanza el runner en modo `Host`, sincronizando el estado de todos los objetos de red. El cliente se conecta usando la infraestructura de matchmaking de Photon y ambos jugadores ingresan a la partida tras completar la selección de avatar.

![Gameplay overview](Documentation/Images/gameplay_overview.png)

---

## Requisitos Técnicos

| Requisito | Detalle |
|---|---|
| Motor | Unity 6000.x LTS |
| Lenguaje | C# |
| Framework de red | Photon Fusion 2 (Player Host Topology) |
| Nombre de sesión | `ChemicalPotions` |
| Jugadores por sesión | 2 (exactamente) |
| TextMeshPro | Requerido (disponible en Unity Package Manager) |
| Plataforma | Windows 10 / 11 |

> Se requiere conexión a Internet para el matchmaking a través de los servidores de Photon. Ambos jugadores pueden estar en redes distintas sin necesidad de reenvío de puertos.

---

## Cómo Ejecutar el Juego

### Host — Crear Sala

1. Abrir el ejecutable (o el proyecto en Unity y entrar en Play Mode).
2. En el menú principal, hacer clic en **"Host"**.
3. El `NetworkManager` lanzará Photon Fusion 2 en modo `GameMode.Host` con el nombre de sesión `ChemicalPotions`.
4. La pantalla pasará al panel de lobby mostrando **"Sala creada — esperando jugador..."**.
5. Cuando el segundo jugador se conecte, ambos serán llevados automáticamente a la pantalla de **selección de avatar**.

![Main menu](Documentation/Images/main_menu.png)

### Cliente — Unirse a una Sala

1. Abrir el ejecutable en otra máquina (o una segunda instancia en la misma).
2. En el menú principal, hacer clic en **"Join"**.
3. Photon Fusion 2 buscará automáticamente la sesión `ChemicalPotions` activa y conectará al jugador.
4. La pantalla pasará al panel de lobby mostrando **"Conectado a la sala"**.

> **Importante:** Si no existe ninguna sesión activa con ese nombre, la conexión fallará. El Host debe iniciar la sala primero.

### Selección de Avatar

Una vez que ambos jugadores estén conectados, se activa la pantalla de selección de avatar. Cada jugador elige un personaje disponible y presiona **"Listo"**. Cuando ambos confirman su selección, la partida inicia automáticamente.

![Avatar selection](Documentation/Images/avatar_selection.png)

---

## Controles

| Acción | Control |
|---|---|
| Mover al personaje | `W A S D` |
| Interactuar / Recoger objeto | `E` |
| Soltar objeto | `Q` |

---

## Arquitectura de Red

El juego usa **Photon Fusion 2** en topología **Player Host**, lo que significa que uno de los jugadores ejecuta simultáneamente la lógica de servidor y juega como cliente activo.

```
[Máquina del Host]
  └── NetworkRunner (GameMode.Host)
        ├── Autoridad de estado sobre todos los NetworkObjects
        ├── Ejecuta OrderManager, VictoryManager
        └── Participa como jugador local

[Máquina Cliente]
  └── NetworkRunner (GameMode.Client)
        └── Recibe el estado replicado y envía inputs locales
```

### Sincronización de Estado

Los objetos de red críticos usan propiedades `[Networked]` de Fusion 2 para replicar su estado automáticamente:

- `NetworkPlayer` — posición, rotación, ingrediente sostenido y estado de la poción.
- `OrderManager` — lista activa de pedidos (`NetworkArray<OrderNetworkData>`) y dinero total acumulado (`TotalMoney`).
- `NetworkAvatarSelection` — selecciones de avatar por slot (`NetworkArray<int>`) y estados de listo (`NetworkArray<NetworkBool>`).
- `PotionHotPlate` — tipo de ingrediente en cocción, temporizadores de cocción y quemado.
- `PotionMixer` — color actual resultante de las mezclas (`CurrentColor`).

### RPCs

La comunicación de acciones puntuales se maneja con `[Rpc]`:

- `RPC_SelectAvatar` — enviado por cualquier cliente a la autoridad de estado para registrar la selección de avatar.
- `RPC_SetReady` — marca al jugador como listo para iniciar.
- `RPC_NotifyAllPlayersReady` — enviado por la autoridad de estado a todos los clientes para arrancar la escena de juego.

### Flujo de Conexión

1. El host lanza `NetworkRunner` en `GameMode.Host`; el cliente en `GameMode.Client`.
2. Photon conecta ambos runners bajo la sesión `ChemicalPotions`.
3. Al detectar 2 jugadores activos (`ExpectedPlayers = 2`), el host instancia el objeto `NetworkAvatarSelection`.
4. Ambos jugadores seleccionan avatar y confirman listos mediante RPCs.
5. Cuando todos están listos, el host carga la escena de juego con `runner.LoadScene(...)`.
6. Se instancian los `NetworkPlayer` para cada `PlayerRef` según el avatar persistido.

---

## Mecánicas de Juego

### Loop Principal

Los jugadores cooperan para completar pedidos que aparecen en el tablero de órdenes. Cada pedido requiere una poción de un color específico. Para entregarla, deben:

1. Recoger el ingrediente primario correcto del dispensador.
2. Llevarlo al **mezclador** si el pedido requiere un color secundario.
3. Transferir el resultado a la **placa caliente** para cocerlo.
4. Entregarlo en la **zona de entrega** antes de que se queme.

![Gameplay screenshot](Documentation/Images/gameplay_screenshot.png)

### Estaciones

- **Dispenser** — suministra ingredientes de tipo primario (Rojo, Azul, Amarillo).
- **PotionMixer** — combina dos ingredientes primarios para producir uno secundario.
- **PotionHotPlate** — cuece la poción durante un tiempo determinado. Si se excede el tiempo de cocción, la poción se quema y no puede entregarse.
- **DeliveryZone** — punto de entrega de pociones; solo acepta pociones en estado `Cooked`.
- **TrashBin** — permite desechar ingredientes o pociones fallidas.

### Condición de Victoria

El `VictoryManager` monitorea cada frame (solo en el servidor) el valor de `OrderManager.TotalMoney`. Cuando este alcanza **$300**, el runner carga la escena de victoria automáticamente para todos los jugadores.

---

## Sistema de Ingredientes y Pociones

### Ingredientes Primarios

| Tipo | Color | Recompensa |
|---|---|---|
| Red | Rojo | $10 |
| Blue | Azul | $10 |
| Yellow | Amarillo | $10 |

### Ingredientes Secundarios (mezclas)

| Combinación | Resultado | Color | Recompensa |
|---|---|---|---|
| Red + Blue | Purple | Morado | $20 |
| Blue + Yellow | Green | Verde | $20 |
| Red + Yellow | Orange | Naranja | $20 |

### Estados de la Poción

```
Raw  →  (cocción dentro del tiempo)  →  Cooked  ✓ entregable
Raw  →  (tiempo excedido)            →  Burned  ✗ no entregable
```

La `PotionHotPlate` muestra una barra de progreso que va de verde (cociendo correctamente) a rojo (quemándose). Un ingrediente del mismo tipo no puede mezclarse consigo mismo.

---

## Selección de Avatar

La pantalla de selección de avatar usa el `NetworkAvatarSelection`, un `NetworkBehaviour` con autoridad de estado en el host. Cada avatar está definido por un `AvatarDefinition` (ScriptableObject) que contiene nombre, prefab de red, imagen de previsualización y color de acento.

Los slots de selección se replican mediante `NetworkArray<int>` (índice de avatar por jugador) y `NetworkArray<NetworkBool>` (estado de listo). Al detectar que todos los jugadores activos tienen selección y están listos, la autoridad de estado invoca `RPC_NotifyAllPlayersReady` hacia todos los clientes, disparando la carga de la escena.

Las selecciones se persisten en el diccionario estático `PersistedSelections` para que el `PlayerSpawner` pueda instanciar el prefab correcto al cargar la nueva escena.

![Avatar selection detail](Documentation/Images/avatar_selection_detail.png)

---

## Flujo Completo del Juego

```
[Menú Principal]
       │
       ├── Host ──────────────► [Lobby — esperando jugador]
       └── Join ──────────────► [Lobby — buscando sala]
                                        │
                               (2 jugadores conectados)
                                        │
                          [Selección de Avatar — ambos jugadores]
                                        │
                            Ambos presionan "Listo"
                                        │
                               RPC_NotifyAllPlayersReady
                                        │
                              [Escena de Juego cargada]
                                        │
                     Jugadores cooperan: recoger → mezclar → cocer → entregar
                                        │
                          TotalMoney >= $300  (chequeado cada frame por el host)
                                        │
                              [Escena de Victoria cargada]
                                        │
                                Botón "Volver al Menú"
                                        │
                             runner.Shutdown() → SceneManager.LoadScene(0)
```

![Victory screen](Documentation/Images/victory_screen.png)

**Condiciones especiales:**

- Si un jugador se desconecta durante la partida, la sesión finaliza ya que el juego requiere exactamente 2 jugadores.
- Si el host cierra la aplicación, el cliente recibe el evento `OnDisconnected` y regresa al menú principal.
- Las pociones quemadas deben desecharse en el `TrashBin` antes de poder recoger nuevos ingredientes.

---

## Estructura de Scripts

```
Scripts/
├── AvatarSelection/
│   ├── AvatarDefinition              ScriptableObject con nombre, prefab, sprite y color de un avatar
│   ├── AvatarRegistry                ScriptableObject que agrupa todos los avatares disponibles
│   ├── AvatarSelectionUI             UI de selección de avatar; escucha eventos de NetworkAvatarSelection
│   ├── AvatarSlotUI                  Slot individual de avatar: preview, borde, indicador de selección
│   └── NetworkAvatarSelection        NetworkBehaviour con autoridad de estado; maneja selecciones y RPCs
│
├── Core/
│   ├── FusionSceneLoader             Implementa ISceneLoader; carga escenas mediante NetworkRunner
│   ├── ReturnToMenu                  Cierra el runner de Fusion y vuelve a la escena del menú
│   └── VictoryManager                Monitorea TotalMoney en el servidor y carga la escena de victoria
│
├── Ingredients/
│   ├── IngredientColorUtility        Mapea IngredientType a Color de Unity; ajusta color según PotionState
│   ├── IngredientIconDatabase        ScriptableObject que asocia cada IngredientType a un Sprite
│   ├── IngredientType                Enum: None, Red, Blue, Yellow, Orange, Green, Purple
│   ├── IngredientUtility             Lógica de mezcla (TryMix), clasificación primaria/secundaria y recompensas
│   └── PotionState                   Enum: Raw, Cooked, Burned
│
├── Interfaces/
│   ├── IAvatarSelectionService       Contrato de consulta y acción sobre la selección de avatares
│   ├── INetworkService               Contrato de inicio de host/cliente y eventos de conexión
│   ├── IPlayerSpawner                Contrato de spawn y despawn de jugadores por PlayerRef
│   └── ISceneLoader                  Contrato de carga de escena de juego
│
├── Network/
│   └── NetworkManager                Singleton; implementa INetworkService e INetworkRunnerCallbacks
│                                     Gestiona el ciclo completo: conexión → selección → spawn → partida
│
├── Orders/
│   ├── DeliveryZone                  Trigger de entrega; delega validación a OrderManager
│   ├── OrderBoardUI                  UI que muestra los pedidos activos y el dinero acumulado
│   ├── OrderCardUI                   Tarjeta individual de pedido: ícono de poción y recompensa
│   └── OrderManager                  NetworkBehaviour con autoridad de estado; genera, valida y completa pedidos
│
├── Player/
│   ├── NetworkPlayer                 NetworkBehaviour principal del jugador: movimiento, interacción y estado de manos
│   ├── PlayerHeldItemVisual          Muestra visualmente el ingrediente que el jugador sostiene
│   ├── PlayerInputData               Estructura de datos de input por frame
│   ├── PlayerInputHandler            Lee el input local y lo empaqueta en PlayerInputData
│   ├── PlayerInteractableDetector    Detecta los objetos interactuables cercanos al jugador
│   ├── PlayerInteractionHandler      Ejecuta la lógica de recogida, depósito y entrega de ingredientes
│   └── PlayerSpawner                 Implementa IPlayerSpawner; instancia el prefab de avatar correcto en spawn/despawn
│
├── Stations/
│   ├── Dispenser                     Fuente de ingredientes primarios; entrega al jugador al interactuar
│   ├── PotionHotPlate                NetworkBehaviour; cuece la poción con temporizador, detecta quemado y muestra progreso
│   ├── PotionMixer                   NetworkBehaviour; combina dos ingredientes primarios en uno secundario
│   └── TrashBin                      Punto de descarte de ingredientes y pociones fallidas
│
└── UI/
    ├── ConnectionUI                  UI alternativa de conexión con feedback de estado (Host / Join)
    └── MainMenuUI                    Gestiona los paneles de menú principal, lobby y carga; escucha INetworkService
```

---

## Integrantes

| Nombre | Rol |
|---|---|
| Jaime Barragán Imbett | Desarrollo |
| Paula Andrea Castro Molina | Desarrollo |

<p align="right"><a href="#readme-top">Volver al inicio</a></p>
