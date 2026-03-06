# Um jogo de Plataforma Noir

Um jogo de plataforma 2D de ação tática e precisão, desenvolvido na Unity 6. Opera sob a filosofia de design "Canhão de Vidro" (*Glass Cannon*), onde a tensão não vem da gestão de vida, mas da execução perfeita de movimentos. Em um mundo segmentado em formato de *grid*, o combate é estritamente binário: você executa ou morre.

---

## 🎥 Gameplay

> **Nota para o Desenvolvedor:** Para adicionar o vídeo aqui no GitHub, basta arrastar o seu arquivo `.mp4` ou `.gif` do seu computador e soltar diretamente dentro da caixa de texto de edição do GitHub. Ele vai gerar o link automaticamente e o vídeo vai rodar direto na página!
> 

---

## ⚔️ Filosofia de Combate
* **Fragilidade Absoluta:** Não existem barras de vida (HP). Qualquer fonte de dano (colisão física, espinhos, lasers) resulta na morte instantânea do personagem.
* **Letalidade Mútua:** Qualquer ataque bem-sucedido contra um oponente também é letal na hora. O jogador nunca "troca dano".

## ⚙️ Mecânicas Principais
* **Dash:** Um impulso direcional de alta velocidade que consome **1 Carga**. Durante o trajeto, o jogador ganha intangibilidade (*i-frames*), permitindo atravessar lasers e ameaças. Se a *hitbox* do Dash cruzar o corpo de um inimigo, ele é destruído.
* **Backstab:** O combate recompensa o flanqueamento. Executar um inimigo desavisado pelas costas elimina a ameaça silenciosamente e restaura **+1 Carga**, incentivando o posicionamento tático em vez da agressividade descuidada.

## ⚖️ Economia e Ciclo de Punição
A gestão de recursos determina a sobrevivência e a punição por falhas:
* **Cargas (Combustível Ofensivo):** Limitadas a um máximo. Essenciais para o Dash.
* **Vínculos (Tentativas):** Limitadas a um máximo. Essenciais para o Soft Respawn.

O sistema de *Respawn* reage à quantidade de Vínculos do jogador:
* **Soft Respawn (A Segunda Chance):** Se morrer com Vínculos > 0, custa 1 Vínculo. O jogador sofre um *Rebobinar Local*, ressurgindo na porta da sala atual. Apenas os inimigos daquela sala específica têm seus estados e posições resetados.
* **Hard Respawn (O Colapso):** Se os Vínculos chegarem a 0, a *run* atual entra em colapso. Ocorre um *Reset Global*: o jogador acorda no último Banco (*Save Point*) que descansou, o progresso não salvo é perdido e todos os inimigos do mundo revivem.

## 🗺️ Arquitetura do Mundo
O level design simula um *dungeon crawler* linear:
* **The Grid (Salas):** O mapa é segmentado em áreas retangulares isoladas.
* **A Gaiola (Câmera):** Utilizando *Cinemachine Confiner*, a câmera é estritamente travada nas paredes da sala atual, ocultando o próximo desafio.
* **Transições:** Cruzar uma porta resulta em um corte de câmera instantâneo e cria um *Checkpoint Invisível* temporário para a sala.

## 👹 Bestiário
Cada inimigo exige uma coreografia específica para ser superado:
* **"Anda" (O Perseguidor):** Patrulheiro terrestre com campo de visão. Ao detectar o jogador, entra em *Chase* (perseguição) em alta velocidade. Seu toque é letal. Deve ser flanqueado (Backstab) ou atravessado com o Dash.
* **"Obser" (A Sentinela):** Sniper de área. Projeta vigilância e, ao travar no jogador, carrega e dispara um feixe de laser letal e contínuo. Exige o uso calculado do Dash para atravessar o raio de luz de forma segura.

**Tecnologias:** Unity 6 • C#
