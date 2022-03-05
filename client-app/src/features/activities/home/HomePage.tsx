import React from "react";
import { Link } from "react-router-dom";
import { Container } from "semantic-ui-react";

export default function HomePage() {
    return (
        <Container style={{margin:10}}>
            <h1>Home Page</h1>
            <h3>Go to <Link to='/activities'>Activities</Link></h3>
        </Container>
    )
}