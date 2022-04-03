# PlaceVRsualizer

A Unity project to visualize in VR Reddit "[Place](https://www.reddit.com/r/place)" spatio-temporal data in 3D.

## Goals

- add 2022 data as soon as they are available.
- have a 3D visualizer with activity in the 3rd axis (for now it's only a 2D image).
- up to date software, because other project is from 2017.
- good performances, because it's VR!

## SQL information

This project doesn't include the original 2017 data that you can find here: https://www.reddit.com/r/redditdata/comments/6640ru/place_datasets_april_fools_2017/. You can use SQLite to treat the data.

Import the csv file ([full dataset gzipped CSV](https://storage.googleapis.com/justin_bassett/place_tiles)) in SQLite. You just have to add `.csv` to the name of the file.

```sql
CREATE TABLE "place_tiles" (
	"ts"	TEXT,
	"user_hash"	TEXT,
	"x_coordinate"	INTEGER,
	"y_coordinate"	INTEGER,
	"color"	INTEGER
)
CREATE INDEX "timestamp" ON "place_tiles" (
	"ts"
)
CREATE INDEX "x" ON "place_tiles" (
	"x_coordinate"
)
CREATE INDEX "y" ON "place_tiles" (
	"y_coordinate"
)
```

Remove lines where `x_coordinate`, `y_coordinate` are `NULL`.

Create a view with data sorted by timestamp and re-export this view as CSV.

```sql
CREATE VIEW place_tiles_sorted_no_user
AS
SELECT ts, x_coordinate, y_coordinate, color FROM place_tiles ORDER BY ts ASC;
```

Due to git lfs limit, the precomputed binaries can be found here: https://drive.google.com/drive/folders/1EV5CrzvZtq_z63eh48XjqnQahSkEy84_?usp=sharing. Download them and put files in the `Data` folder.

## Credits

This project was inspiered by [GregBahm/PlaceViewer](https://github.com/GregBahm/PlaceViewer).
