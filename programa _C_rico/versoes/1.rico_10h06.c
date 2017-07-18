#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define TAMANHO_TEXTO 41

int main(){

	FILE *newfile = fopen("newfile.txt", "a"); //cria um novo ficheiro
	printf("--novo ficheiro aberto\n");
	FILE *parameters = fopen("machineparameters.txt", "r"); //abre o ficheiro dos parametros
	char line[TAMANHO_TEXTO], found = 0; //determina que cada linha copiada terá 65 caracteres
	char texto[] = "  Linha de serviço                       \n";

  	if (parameters == NULL)  //se retornar NULL ao abrir o ficheiro termina o programa
  	{
  		perror("fdopen");
  		printf("!!!ERRO a abrir os parametros!!!\n");
  		fclose(parameters);
  		exit(0);
  	}

//cada linha terá pelo menos 70 caracteres


//	fseek(parameters, 18, SEEK_SET);	//salta para o xº caracter do ficheiro
	printf("--Vou procurar: ");			//indica o texto que vai procurar
	puts(texto);
	printf("--A iniciar procura\n");
	while (fgets(line, TAMANHO_TEXTO, parameters))//lê uma linha do ficheiro (ou 500 caracteres, o que chegar primeiro)
	{
		puts(line);						//mostra o que encontrou quando passou no while
		printf("__nova tentativa de comparacao__\n");
		if (strcmp(line, texto) == 0)	//este é o texto que procuramos, se for igual entra no IF
    	{
       		printf("--texto encontrado. A copiar...\n");
			fputs(line, newfile);		//adiciona a nova linha ao ficheiro
//        	fprintf(newfile, "\n");
        	printf("--escrita ok!\n--Programa vai terminar\n");
        	found = 1;					//se fez uma comparação com sucesso nao vai
        	break;						// deixar entrar no if depois do while
    	}else{
    		printf("--nao encontrou\n");
    	}
//    	found = 0;						//garante que o "End of file without..." so vai aparecer se
	}									// entrar no while, pelo menos uma vez
	if (found == 0){					//se nao encontrei igual, vai informar o utilizador		
		printf("--End of file without match\n--Programa vai terminar\n");
	}
	
	fclose(parameters);
	fclose(newfile);


}
