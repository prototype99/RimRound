
# red_template = "red.png"

for file in *; do
	echo $file
	if [ -f "${file}" ]; then
		cp "red.png" "new/${file}"
	fi
done