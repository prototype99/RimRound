git commit
if [ $? -eq 0 ]; then
    echo "Commit successful"
    TZ=UTC0 git --no-pager log --date=local --pretty=format:"==================================================%nCommit Hash: %H%nAuthor: %an%nDate: %ad%n%n%B" > changelog.txt
    git add changelog.txt
    git commit --amend --no-edit
else
    echo "Aborted early."
fi
