.PHONY: all build clean install

all: build

build:
	dotnet build
	mkdir -p build/EnglishDolphin
	cp bin/Debug/netstandard2.1/EnglishDolphin.dll build/EnglishDolphin
	cp -R res/ build/EnglishDolphin/
	zip EnglishDolphin.zip -r build/EnglishDolphin
	
clean:
	rm -rf build bin obj EnglishDolphin.zip

install: build
	rm -r $(HOME)/.local/share/Steam/steamapps/common/いるかにうろこがないわけ/BepInEx/plugins/EnglishDolphin
	cp -r build/EnglishDolphin $(HOME)/.local/share/Steam/steamapps/common/いるかにうろこがないわけ/BepInEx/plugins/
