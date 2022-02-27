import { observer } from "mobx-react-lite";
import React, { ChangeEvent, useState } from "react";
import { Button, Form, Segment } from "semantic-ui-react";
import { useStore } from "../../../app/stores/store";

export default observer(function ActivityForm() {
    const { activityStore } = useStore();
    const { selectedActivity, closeForm, createActivity, updateActivity, loading } = activityStore;
    const initialState = selectedActivity ?? {
        id: '',
        title: '',
        date: '',
        category: '',
        description: '',
        city: '',
        venue: ''
    };

    const [activity, setActivity] = useState(initialState);

    function handleSubmit() {
        activity.id ? updateActivity(activity) : createActivity(activity);
    }

    function handleInputChange(event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) {
        const { name, value } = event.target;
        setActivity({ ...activity, [name]: value });
    }

    return (
        <Segment clearing>
            <Form onSubmit={handleSubmit} autoComplete='off'>
                <Form.Input label='Title' placeholder='Title' value={activity.title} name='title' onChange={handleInputChange} />
                <Form.TextArea label='Description' placeholder='Description' value={activity.description} name='description' onChange={handleInputChange} />
                <Form.Input label='Category' placeholder='Category' value={activity.category} name='category' onChange={handleInputChange} />
                <Form.Input label='date' type='date' placeholder='Date' value={activity.date} name='date' onChange={handleInputChange} />
                <Form.Input label='City' placeholder='City' value={activity.city} name='city' onChange={handleInputChange} />
                <Form.Input label='Venue' placeholder='Venue' value={activity.venue} name='venue' onChange={handleInputChange} />
                <Button onClick={closeForm} floated='right' type='button' content='Cancel' value={activity.title} onChange={handleInputChange} />
                <Button loading={loading} loated='right' positive type='submit' content='Submit' />
            </Form>
        </Segment>
    )
});