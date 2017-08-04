extern crate id3;

use id3::Tag;

fn main() {
    let tag = Tag::read_from_path("music.mp3").unwrap();
    println!("{}", tag.artist().unwrap());
}
