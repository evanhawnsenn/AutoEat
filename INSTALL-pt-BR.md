# Instalar o AutoEat

Este guia serve para jogadores de **Space Engineers 1** que usam o carregador de plugins **Pulsar** no Windows.

## Opção recomendada — após aprovação no PluginHub

Quando a proposta do AutoEat for aprovada no PluginHub:

1. Abra o Pulsar.
2. Atualize a lista de plugins, se houver essa opção.
3. Procure por **AutoEat**.
4. Marque-o como ativado no perfil que você usa para jogar.
5. Inicie o Space Engineers pelo Pulsar.

O AutoEat aparece no Terminal do personagem, na aba **Painel de controle**. Use `Ctrl + Alt + U` para ativar ou desativar rapidamente a alimentação automática.

## Instalação manual — enquanto a aprovação não chega

1. Abra a [release mais recente](https://github.com/evanhawnsenn/AutoEat/releases/latest).
2. Em **Assets**, baixe **Source code (zip)**.
3. Extraia o ZIP em uma pasta permanente, por exemplo `C:\Games\PulsarPlugins\AutoEat`.
   - Não extraia em `Downloads` se você costuma limpar essa pasta.
4. No Pulsar, adicione a pasta extraída como uma **fonte local/de desenvolvimento de plugin**.
5. Ative esse plugin no perfil atual do Pulsar.
6. Inicie o Space Engineers pelo Pulsar.

### Como conferir se carregou

- Entre em um mundo e abra o Terminal do seu personagem.
- Vá em **Painel de controle**.
- A seção **AUTOEAT** deve aparecer somente no Terminal do personagem — não nos terminais de blocos da base.

## Configuração inicial

- **Consumo automático:** liga ou desliga o recurso.
- **Comer abaixo de:** porcentagem de fome em que o personagem tenta comer.
- **Avisar sem comida:** ativa o alerta caso não exista alimento no inventário pessoal.
- **Avisar abaixo de:** porcentagem de fome que dispara o alerta.
- **Prioridade de comida:** organiza a ordem em que os alimentos serão usados.

O primeiro alerta aparece imediatamente ao atingir o limite configurado e volta a aparecer a cada 30 segundos enquanto não houver comida.

## Se não funcionar

1. Confirme que o jogo foi aberto pelo Pulsar, e não diretamente pelo Steam.
2. Confirme que o AutoEat está ativado no perfil correto do Pulsar.
3. Feche o jogo, reinicie o Pulsar e tente novamente.
4. Não ative ao mesmo tempo o antigo mod de mundo chamado AutoEat.
5. Abra o painel de depuração do Pulsar e envie uma captura dos erros ao autor.

## Observações

- O plugin procura alimentos compatíveis no inventário pessoal do personagem.
- A interface usa português quando o jogo está em português e inglês nos demais idiomas.
- AutoEat é um projeto independente, criado por Denis Martins com auxílio do OpenAI Codex (IA).
